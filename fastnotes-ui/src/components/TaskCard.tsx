import type { TaskDto } from '../api/tasks';
import '../ui.css';

import CalendarIcon from '../../public/ui/icons/calendar.svg?react';
import UserIcon from '../../public/ui/icons/user.svg?react';


type Props = {
    task: TaskDto;
};

const formatDate = (date: Date) =>
    date.toLocaleDateString(undefined, {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
    });

export const TaskCard = ({ task }: Props) => {
    const dueDate = task.dueDate ? new Date(task.dueDate) : null;

    return (
        <li className="ui-task-card">
            <header className="ui-task-header">
                <h3 className="ui-task-title">
                    {task.title}
                </h3>

                <span className="ui-badge" aria-label="Voice note">
          <span aria-hidden>🎙</span>
          Voice
        </span>
            </header>

            {task.description && (
                <p className="ui-task-description">
                    {task.description}
                </p>
            )}

            <footer className="ui-task-footer">
                {dueDate && (
                    <time dateTime={dueDate.toISOString()} className="ui-badge">
                        <CalendarIcon className="ui-icon" />
                        {formatDate(dueDate)}
                    </time>
                )}

                <span className="ui-badge">
                    <UserIcon className="ui-icon" />
                    {task.assignedTo}
        </span>
            </footer>
        </li>
    );
};
