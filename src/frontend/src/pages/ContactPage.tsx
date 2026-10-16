
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import { Textarea } from "@/components/ui/textarea";
import {
  CheckCircle,
  Clock,
  Headphones,
  Mail,
  MapPin,
  MessageSquare,
  Phone,
  Send,
  Shield,
} from "lucide-react";
import { useState } from "react";

export default function ContactPage() {
  const [formData, setFormData] = useState({
    name: "",
    email: "",
    subject: "",
    message: "",
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSubmitted, setIsSubmitted] = useState(false);

  const handleInputChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    await new Promise((resolve) => setTimeout(resolve, 1000));

    setIsSubmitting(false);
    setIsSubmitted(true);

    setTimeout(() => {
      setIsSubmitted(false);
      setFormData({ name: "", email: "", subject: "", message: "" });
    }, 3000);
  };

  const contactInfo = [
    {
      icon: Mail,
      title: "Envie um e-mail",
      details: ["hello@bloomshop.com", "support@bloomshop.com"],
      description: "Escreva para nós quando quiser",
    },
    {
      icon: Phone,
      title: "Ligue para nós",
      details: ["+1 (555) 123-4567", "+1 (555) 987-6543"],
      description: "De segunda a sexta, das 8h às 17h",
    },
    {
      icon: MapPin,
      title: "Visite-nos",
      details: ["123 Fashion Street", "Style City, SC 12345"],
      description: "Venha nos dar um oi no escritório",
    },
    {
      icon: Clock,
      title: "Horário de atendimento",
      details: ["Segunda a sexta: 9h às 18h", "Sábado: 10h às 16h"],
      description: "Domingo: fechado",
    },
  ];

  const features = [
    {
      icon: Headphones,
      title: "Suporte 24 horas",
      description: "Ajuda sempre que você precisar",
    },
    {
      icon: MessageSquare,
      title: "Resposta rápida",
      description: "Respondemos em até 2 horas",
    },
    {
      icon: Shield,
      title: "Seguro e privado",
      description: "Seus dados estão seguros conosco",
    },
  ];

  return (
    <div className="bg-background">
      <section className="py-16 lg:py-24 bg-gradient-to-br from-primary/5 to-accent/5">
        <div className="container mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center max-w-3xl mx-auto">
            <Badge className="mb-6 bg-primary text-primary-foreground">
              Fale com a gente
            </Badge>
            <h1 className="text-4xl lg:text-6xl font-bold text-foreground mb-6">
              Queremos{" "}
              <span className="text-primary block lg:inline lg:ml-4">
                ouvir você
              </span>
            </h1>
            <p className="text-lg text-muted-foreground max-w-2xl mx-auto">
              Tem uma dúvida, uma sugestão ou só quer dar um oi? Estamos aqui para
              ajudar e vamos adorar falar com você.
            </p>
          </div>
        </div>
      </section>

      <section className="py-16 lg:py-24">
        <div className="container mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid lg:grid-cols-3 gap-12">
            <div className="lg:col-span-2">
              <Card>
                <CardHeader>
                  <CardTitle className="text-2xl font-bold text-foreground">
                    Envie uma mensagem
                  </CardTitle>
                  <p className="text-muted-foreground">
                    Preencha o formulário abaixo e responderemos o mais rápido
                    possível.
                  </p>
                </CardHeader>
                <CardContent>
                  <form onSubmit={handleSubmit} className="space-y-6">
                    <div className="grid sm:grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <label
                          htmlFor="name"
                          className="text-sm font-medium text-foreground"
                        >
                          Seu nome
                        </label>
                        <Input
                          id="name"
                          name="name"
                          type="text"
                          placeholder="João da Silva"
                          value={formData.name}
                          onChange={handleInputChange}
                          required
                          className="bg-background border-border"
                        />
                      </div>

                      <div className="space-y-2">
                        <label
                          htmlFor="email"
                          className="text-sm font-medium text-foreground"
                        >
                          Seu e-mail
                        </label>
                        <Input
                          id="email"
                          name="email"
                          type="email"
                          placeholder="joao@exemplo.com"
                          value={formData.email}
                          onChange={handleInputChange}
                          required
                          className="bg-background border-border"
                        />
                      </div>
                    </div>

                    <div className="space-y-2">
                      <label
                        htmlFor="subject"
                        className="text-sm font-medium text-foreground"
                      >
                        Assunto
                      </label>
                      <Input
                        id="subject"
                        name="subject"
                        type="text"
                        placeholder="Como podemos ajudar?"
                        value={formData.subject}
                        onChange={handleInputChange}
                        required
                        className="bg-background border-border"
                      />
                    </div>

                    <div className="space-y-2">
                      <label
                        htmlFor="message"
                        className="text-sm font-medium text-foreground"
                      >
                        Sua mensagem
                      </label>
                      <Textarea
                        id="message"
                        name="message"
                        placeholder="Conte mais sobre a sua dúvida ou problema..."
                        rows={6}
                        value={formData.message}
                        onChange={handleInputChange}
                        required
                        className="bg-background border-border resize-none"
                      />
                    </div>

                    <Button
                      type="submit"
                      size="lg"
                      disabled={isSubmitting || isSubmitted}
                      className="w-full sm:w-auto bg-primary text-primary-foreground hover:bg-primary/90"
                    >
                      {isSubmitting ? (
                        <div className="flex items-center gap-2">
                          <div className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
                          Enviando...
                        </div>
                      ) : isSubmitted ? (
                        <div className="flex items-center gap-2">
                          <CheckCircle className="h-4 w-4" />
                          Mensagem enviada!
                        </div>
                      ) : (
                        <div className="flex items-center gap-2">
                          <Send className="h-4 w-4" />
                          Enviar mensagem
                        </div>
                      )}
                    </Button>
                  </form>
                </CardContent>
              </Card>
            </div>

            <div className="space-y-8">
              <Card>
                <CardHeader>
                  <CardTitle className="text-xl font-semibold">
                    Informações de contato
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-6">
                  {contactInfo.map((info, index) => (
                    <div key={index} className="flex items-start gap-4">
                      <div className="p-2 bg-primary/10 rounded-lg">
                        <info.icon className="h-5 w-5 text-primary" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <h3 className="font-semibold text-foreground mb-1">
                          {info.title}
                        </h3>
                        {info.details.map((detail, idx) => (
                          <p
                            key={idx}
                            className="text-sm text-muted-foreground"
                          >
                            {detail}
                          </p>
                        ))}
                        <p className="text-xs text-muted-foreground mt-1">
                          {info.description}
                        </p>
                      </div>
                    </div>
                  ))}
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="text-xl font-semibold">
                    Por que falar com a gente?
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  {features.map((feature, index) => (
                    <div key={index}>
                      <div className="flex items-start gap-3">
                        <div className="p-1 bg-accent/10 rounded">
                          <feature.icon className="h-4 w-4 text-accent-foreground" />
                        </div>
                        <div>
                          <h4 className="font-medium text-foreground text-sm">
                            {feature.title}
                          </h4>
                          <p className="text-xs text-muted-foreground">
                            {feature.description}
                          </p>
                        </div>
                      </div>
                      {index < features.length - 1 && (
                        <Separator className="mt-4" />
                      )}
                    </div>
                  ))}
                </CardContent>
              </Card>
            </div>
          </div>
        </div>
      </section>

      <section className="py-16 lg:py-24 bg-muted/30">
        <div className="container mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <Badge variant="outline" className="mb-6">
              FAQ
            </Badge>
            <h2 className="text-3xl lg:text-4xl font-bold text-foreground mb-4">
              Perguntas frequentes
            </h2>
            <p className="text-lg text-muted-foreground max-w-2xl mx-auto">
              Encontre respostas rápidas para as dúvidas mais comuns sobre nossos
              produtos e serviços.
            </p>
          </div>

          <div className="grid md:grid-cols-2 gap-8 max-w-4xl mx-auto">
            {[
              {
                question: "Como funciona a entrega?",
                answer:
                  "O frete é grátis em compras acima de R$ 50. A entrega padrão leva de 3 a 5 dias úteis.",
              },
              {
                question: "Como acompanho o meu pedido?",
                answer:
                  "Quando o pedido for enviado, você recebe por e-mail um código de rastreamento para acompanhar a entrega.",
              },
              {
                question: "Qual é a política de devolução?",
                answer:
                  "Aceitamos devoluções em até 30 dias após a compra. Os produtos devem estar nas condições originais.",
              },
              {
                question: "Vocês entregam fora do Brasil?",
                answer:
                  "Sim, entregamos no mundo todo. O frete internacional varia conforme o destino.",
              },
            ].map((faq, index) => (
              <Card key={index} className="hover:shadow-md transition-shadow">
                <CardContent className="p-6">
                  <h3 className="font-semibold text-foreground mb-3">
                    {faq.question}
                  </h3>
                  <p className="text-sm text-muted-foreground">{faq.answer}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      <section className="py-16 lg:py-24">
        <div className="container mx-auto px-4 sm:px-6 lg:px-8">
          <Card className="bg-gradient-to-r from-primary/10 to-accent/10 border-primary/20">
            <CardContent className="p-12 text-center">
              <h2 className="text-3xl lg:text-4xl font-bold text-foreground mb-4">
                Ainda tem dúvidas?
              </h2>
              <p className="text-lg text-muted-foreground mb-8 max-w-2xl mx-auto">
                Não encontrou o que procurava? Nossa equipe de atendimento está
                aqui para ajudar.
              </p>

              <div className="flex flex-col sm:flex-row gap-4 justify-center">
                <Button
                  size="lg"
                  className="bg-primary text-primary-foreground hover:bg-primary/90"
                >
                  <Phone className="h-4 w-4 mr-2" />
                  Ligue agora
                </Button>

                <Button size="lg" variant="outline">
                  <Mail className="h-4 w-4 mr-2" />
                  Chat ao vivo
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      </section>
    </div>
  );
}
