// Sem login, o cliente é um GUID anônimo gerado uma vez e guardado no
// navegador, para que os pedidos seguintes saiam com o mesmo id.
const CUSTOMER_KEY = "customerId";

export function getCustomerId(): string {
  const saved = localStorage.getItem(CUSTOMER_KEY);
  if (saved) return saved;

  const id = crypto.randomUUID();
  localStorage.setItem(CUSTOMER_KEY, id);
  return id;
}
