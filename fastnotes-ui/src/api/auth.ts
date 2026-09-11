import axios from "axios";

const API_BASE_URL = import.meta.env.API_BASE_URL;

export interface LoginRequest {
    userId: string;
    canWrite: boolean;
}
const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api'
});
export interface LoginResponse {
    access_token: string;
    token_type: string;
    expires_in: number;
}

export const login = async (
    request: LoginRequest
): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>(
        `auth/login`,
        request
    );

    return response.data;
};