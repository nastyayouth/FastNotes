import { TaskList } from './components/TaskList';

function App() {
    return (
        <div className="app-shell">
            <main className="app-content">
                <h1 className="app-title">FastNotes</h1>
                <TaskList/>
            </main>
        </div>

    );
}

export default App;
