#!/usr/bin/env python3
"""Move as mensagens de uma fila de erro do MassTransit de volta para a fila de origem.

Uso:
    python3 docs/runbooks/mover-fila-de-erro.py payment-approved_error
    python3 docs/runbooks/mover-fila-de-erro.py payment-approved_error --dry-run

Usa só a API de gerenciamento do RabbitMQ (http://localhost:15672, guest/guest por padrão;
troque com RABBITMQ_URL, RABBITMQ_USER e RABBITMQ_PASSWORD). Cada mensagem é lida da fila de
erro sem devolução e publicada de novo na fila de origem (o nome sem "_error"), com as mesmas
propriedades. É seguro: o Payments não cobra de novo uma tentativa já registrada, e o Orders
ignora resultado para pedido que já tem desfecho. Veja docs/runbooks/filas-de-erro.md.
"""
import base64
import json
import os
import sys
import urllib.parse
import urllib.request

BASE = os.environ.get("RABBITMQ_URL", "http://localhost:15672").rstrip("/")
USER = os.environ.get("RABBITMQ_USER", "guest")
PASSWORD = os.environ.get("RABBITMQ_PASSWORD", "guest")
BATCH = 1000
AUTH = "Basic " + base64.b64encode(f"{USER}:{PASSWORD}".encode()).decode()


def call(method, path, body=None):
    request = urllib.request.Request(BASE + path, method=method,
                                     data=None if body is None else json.dumps(body).encode(),
                                     headers={"Content-Type": "application/json", "Authorization": AUTH})
    with urllib.request.urlopen(request) as response:
        return json.load(response)


def main():
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    dry_run = "--dry-run" in sys.argv
    if len(args) != 1 or not args[0].endswith("_error"):
        sys.exit("Informe a fila de erro, por exemplo: payment-approved_error")
    error_queue = args[0]
    source_queue = error_queue[: -len("_error")]
    vhost = urllib.parse.quote("/", safe="")

    # A contagem da API de gerenciamento é atualizada a cada poucos segundos: serve para informar,
    # não para decidir. A leitura abaixo pega o que estiver de fato na fila, até BATCH mensagens.
    queue = call("GET", f"/api/queues/{vhost}/{urllib.parse.quote(error_queue)}")
    print(f"{error_queue}: cerca de {queue.get('messages', 0)} mensagem(ns); destino: {source_queue}")
    if dry_run:
        return

    # ack_requeue_false tira a mensagem da fila de erro; ela é publicada de novo logo em seguida.
    messages = call("POST", f"/api/queues/{vhost}/{urllib.parse.quote(error_queue)}/get",
                    {"count": BATCH, "ackmode": "ack_requeue_false", "encoding": "auto"})
    moved = 0
    for message in messages:
        result = call("POST", f"/api/exchanges/{vhost}/amq.default/publish", {
            "routing_key": source_queue,
            "properties": message.get("properties", {}),
            "payload": message["payload"],
            "payload_encoding": message.get("payload_encoding", "string"),
        })
        if not result.get("routed"):
            sys.exit(f"Mensagem não roteada para {source_queue}; {moved} movida(s) até aqui.")
        moved += 1
    print(f"{moved} mensagem(ns) movida(s) para {source_queue}." if moved else "Nenhuma mensagem na fila de erro.")


if __name__ == "__main__":
    main()
