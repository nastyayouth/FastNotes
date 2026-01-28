import { useEffect, useState } from 'react';
import { getTasks } from '../api/tasks';
import type { TaskDto } from '../api/tasks';
import { TaskCard } from './TaskCard';
import SparkleIcon from '../../public/ui/icons/sparkle.svg?react';

type DateFilter = 'all' | 'today' | 'tomorrow' | 'week' | 'month';
const FILTERS: { key: DateFilter; label: string }[] = [
    { key: 'all', label: 'All' },
    { key: 'today', label: 'Today' },
    { key: 'tomorrow', label: 'Tomorrow' },
    { key: 'week', label: 'Week' },
    { key: 'month', label: 'Month' },
];
export const TaskList = () => {
    const [tasks, setTasks] = useState<TaskDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [filter, setFilter] = useState<DateFilter>('all');

    useEffect(() => {
        getTasks()
            .then(data => setTasks(data))
            .catch(() =>
                setError('Cannot load tasks. Please try again later.')
            )
            .finally(() => setLoading(false));
    }, []);

    if (loading) {
        return (
            <p className="py-6 text-sm text-gray-500">
                Loading tasks…
            </p>
        );
    }

    if (error) {
        return (
            <p className="py-6 text-sm text-red-500">
                {error}
            </p>
        );
    }

    if (tasks.length === 0) {
        return (
            <div className="py-10 text-center text-gray-500">
                <p className="text-base">No tasks yet</p>
                <p className="mt-1 text-sm text-gray-600">
                    Create your first task
                </p>
            </div>
        );
    }

    const isSameDay = (a: Date, b: Date) =>
        a.getFullYear() === b.getFullYear() &&
        a.getMonth() === b.getMonth() &&
        a.getDate() === b.getDate();

    const isTomorrow = (date: Date) => {
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        return isSameDay(date, tomorrow);
    };

    const isThisWeek = (date: Date) => {
        const now = new Date();
        const startOfWeek = new Date(now);
        startOfWeek.setDate(now.getDate() - now.getDay());

        const endOfWeek = new Date(startOfWeek);
        endOfWeek.setDate(startOfWeek.getDate() + 7);

        return date >= startOfWeek && date < endOfWeek;
    };

    const isThisMonth = (date: Date) => {
        const now = new Date();
        return (
            date.getFullYear() === now.getFullYear() &&
            date.getMonth() === now.getMonth()
        );
    };

    const filteredTasks = tasks.filter(task => {
        if (!task.dueDate) return false;

        const dueDate = new Date(task.dueDate);
        const today = new Date();

        switch (filter) {
            case 'today':
                return isSameDay(dueDate, today);
            case 'tomorrow':
                return isTomorrow(dueDate);
            case 'week':
                return isThisWeek(dueDate);
            case 'month':
                return isThisMonth(dueDate);
            default:
                return true;
        }
    });

    return (
        <div className="mb-6 inline-flex rounded-xl bg-gray-800/80 p-0.5">
            {/* Filters */}
            <div className="ui-filter">
                {FILTERS.map(f => (
                    <button
                        key={f.key}
                        className={`ui-filter-button ${filter === f.key ? 'is-active' : ''}`}
                        onClick={() => setFilter(f.key)}
                    >
                        {f.label}
                    </button>
                ))}
            </div>


            <h2 className="mb-3 text-lg font-semibold text-gray-200">
                Tasks
            </h2>

            <ul className="space-y-2">
                {filteredTasks.length === 0 ? (
                    <div
                        className="
              rounded-lg
              border border-dashed border-gray-800
              p-6
              text-center
              text-gray-500
            "
                    >
                        <p className="text-base"> 
                            <SparkleIcon className="ui-icon-sparkle" />
                            No tasks here
                        </p>
                        <p className="mt-1 text-sm text-gray-600">
                            Try another filter or create a new task
                        </p>
                    </div>
                ) : (
                    filteredTasks.map(task => (
                        <TaskCard key={task.id} task={task}/>
                    ))
                )}
            </ul>
        </div>
    );
};
