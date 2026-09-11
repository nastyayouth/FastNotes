import { api } from './client';

export interface TaskDto {
    id: number;
    title: string;
    description: string;
    assignedTo: string;
    dueDate: string;
    isConfirmed: boolean;
}
const JWT_KEY = 'fastnotes.jwt';

api.interceptors.request.use(config => {
    const token = localStorage.getItem(JWT_KEY);
    console.log('JWT from storage at tasks.ts:', token);
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export const getTasks = async (): Promise<TaskDto[]> => {
    const response = await api.get('/Tasks');
    return response.data;
};

export const createTask = async (task: Omit<TaskDto, 'id'>): Promise<TaskDto> => {
    const response = await api.post('/Tasks', task);
    return response.data;
};
