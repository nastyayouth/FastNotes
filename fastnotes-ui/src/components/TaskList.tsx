import { useEffect, useState } from 'react';
import { getTasks } from '../api/tasks';
import type { TaskDto } from '../api/tasks';


export const TaskList = () => {
    const [tasks, setTasks] = useState<TaskDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        getTasks().then(data => {
            setTasks(data);
            setLoading(false);
        });
    }, []);

    if (loading) return <p className="text-gray-500">Загрузка...</p>;

    return (
        <div className="p-4">
            <h2 className="text-xl font-semibold mb-4">Список задач</h2>
            <ul className="space-y-3">
                {tasks.map(task => (
                    <li key={task.id} className="border rounded p-3 shadow-sm">
                        <div className="font-bold">{task.title}</div>
                        <div>{task.description}</div>
                        <div className="text-sm text-gray-600">
                            До: {new Date(task.dueDate).toLocaleDateString()} — Ответственный: {task.assignedTo}
                        </div>
                    </li>
                ))}
            </ul>
        </div>
    );
};
