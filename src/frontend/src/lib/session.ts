// 401 (ou AUTH_NOT_AUTHENTICATED no gateway): a sessão venceu (ou o token foi recusado) e o cliente
// precisa entrar de novo.
export class SessionExpiredError extends Error {
  constructor() {
    super("Sessão expirada");
  }
}
