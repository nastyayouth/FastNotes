import axios from 'axios';
import { vi, describe, it, expect } from 'vitest';
import { getTasks, createTask, TaskDto } from './tasks';

vi.mock('axios');
const mockedAxios = vi.mocked(axios, true);

describe('tasks api', () => {
  it('getTasks calls axios and returns data', async () => {
    const data: TaskDto[] = [
      { id: 1, title: 'T', description: 'D', assignedTo: 'A', dueDate: '2024-01-01', isConfirmed: false }
    ];
    mockedAxios.get.mockResolvedValue({ data });

    const result = await getTasks();
    expect(result).toEqual(data);
    expect(mockedAxios.get).toHaveBeenCalledWith('/api/tasks');
  });

  it('createTask posts data', async () => {
    const input = { title: 'T', description: 'D', assignedTo: 'A', dueDate: '2024-01-01', isConfirmed: false };
    const response: TaskDto = { id: 2, ...input } as TaskDto;
    mockedAxios.post.mockResolvedValue({ data: response });

    const result = await createTask(input);
    expect(result).toEqual(response);
    expect(mockedAxios.post).toHaveBeenCalledWith('/api/tasks', input);
  });
});
