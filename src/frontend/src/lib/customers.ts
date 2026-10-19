import { graphqlRequest } from "./graphql";

// Cadastro do cliente pelo gateway GraphQL (subgraph do Customers). O cliente vem do token: não há id
// na consulta. Sem sessão válida, as funções lançam SessionExpiredError (de graphql.ts).

export interface CustomerAddress {
  street: string;
  number: string;
  complement: string | null;
  district: string;
  city: string;
  state: string;
  zipCode: string;
}

// Telefone, CPF e CEP voltam só com dígitos; a loja formata para exibir. Sem cadastro salvo,
// só o e-mail vem preenchido.
export interface CustomerProfile {
  email: string;
  fullName: string | null;
  phone: string | null;
  cpf: string | null;
  address: CustomerAddress | null;
}

export type SaveProfileRequest = Omit<CustomerProfile, "email">;

export type SaveProfileResult =
  | { ok: true; profile: CustomerProfile }
  // Erros por campo, nas chaves de sempre ("cpf", "address.zipCode").
  | { ok: false; errors: Record<string, string[]> };

const PROFILE_FIELDS =
  "email fullName phone cpf address { street number complement district city state zipCode }";

export async function getProfile(accessToken: string, signal?: AbortSignal): Promise<CustomerProfile> {
  const { me } = await graphqlRequest<{ me: CustomerProfile }>(`query Me { me { ${PROFILE_FIELDS} } }`, undefined, {
    accessToken,
    signal,
  });
  return me;
}

const UPDATE_PROFILE = `mutation UpdateMyProfile($input: UpdateMyProfileInput!) {
  updateMyProfile(input: $input) {
    profile { ${PROFILE_FIELDS} }
    errors { field messages }
  }
}`;

// O cadastro salvo, ou os erros por campo sem nada gravado.
export async function saveProfile(
  profile: SaveProfileRequest,
  accessToken: string
): Promise<SaveProfileResult> {
  const { updateMyProfile } = await graphqlRequest<{
    updateMyProfile: { profile: CustomerProfile | null; errors: { field: string; messages: string[] }[] };
  }>(UPDATE_PROFILE, { input: profile }, { accessToken });

  if (updateMyProfile.profile) return { ok: true, profile: updateMyProfile.profile };
  return {
    ok: false,
    errors: Object.fromEntries(updateMyProfile.errors.map((e) => [e.field, e.messages])),
  };
}
