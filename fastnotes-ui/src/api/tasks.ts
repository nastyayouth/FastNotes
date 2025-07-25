import axios from 'axios';

export interface TaskDto {
    id: number;
    title: string;
    description: string;
    assignedTo: string;
    dueDate: string;
    isConfirmed: boolean;
}

export const getTasks = async (): Promise<TaskDto[]> => {
    const response = await axios.get('/api/tasks');
    return response.data;
};

export const createTask = async (task: Omit<TaskDto, 'id'>): Promise<TaskDto> => {
    const response = await axios.post('/api/tasks', task);
    return response.data;
};
