import { api } from './client';

export interface LoginRequest {
    userId: string;
    canWrite: boolean;
}

export interface LoginResponse {
    access_token: string;
    token_type: string;
    expires_in: number;
}

export const login = async (
    request: LoginRequest
): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>(
        '/auth/login',
        request
    );

    return response.data;
};
