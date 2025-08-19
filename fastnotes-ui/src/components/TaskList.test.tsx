import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { TaskList } from './TaskList';
import { getTasks } from '../api/tasks';

vi.mock('../api/tasks');
const mockedGetTasks = getTasks as any;

describe('TaskList', () => {
  it('renders tasks returned by api', async () => {
    mockedGetTasks.mockResolvedValue([
      { id: 1, title: 'A', description: 'B', assignedTo: 'C', dueDate: new Date().toISOString(), isConfirmed: true }
    ]);

    render(<TaskList />);
    expect(screen.getByText(/Загрузка/)).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText('A')).toBeInTheDocument();
    });
  });
});
