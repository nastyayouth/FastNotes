import axios, { AxiosError } from 'axios';

const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL || '/api'
});

export interface TaskDto {
    id: number;
    title: string;
    description: string;
    assignedTo: string;
    dueDate: string;
    isConfirmed: boolean;
}

export const getTasks = async (): Promise<TaskDto[]> => {
    try {
        const response = await api.get('/tasks');
        return response.data;
    } catch (error) {
        if (error instanceof AxiosError) {
            throw error.response?.data || error;
        }
        throw error;
    }
};

export const createTask = async (task: Omit<TaskDto, 'id'>): Promise<TaskDto> => {
    try {
        const response = await api.post('/tasks', task);
        return response.data;
    } catch (error) {
        if (error instanceof AxiosError) {
            throw error.response?.data || error;
        }
        throw error;
    }
};
