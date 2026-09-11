import axios from 'axios';

const configuredApiBaseUrl = import.meta.env.VITE_API_BASE_URL;

const getApiBaseUrl = () => {
    if (!configuredApiBaseUrl || configuredApiBaseUrl === 'undefined') {
        return '/api';
    }

    const trimmedBaseUrl = configuredApiBaseUrl.trim();

    if (!trimmedBaseUrl || trimmedBaseUrl === 'undefined') {
        return '/api';
    }

    return trimmedBaseUrl.replace(/\/$/, '');
};

export const api = axios.create({
    baseURL: getApiBaseUrl()
});
