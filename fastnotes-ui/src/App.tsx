import { useMemo, useState } from 'react';
import { TaskList } from './components/TaskList';

const JWT_KEY = 'fastnotes.jwt';
function App() {
    const [tokenInput, setTokenInput] = useState('');
    const [storedToken, setStoredToken] = useState(() => localStorage.getItem(JWT_KEY) ?? '');
    const isAuthenticated = useMemo(() => storedToken.length > 0, [storedToken]);

    const handleSaveToken = (event: React.FormEvent) => {
        event.preventDefault();
        const trimmed = tokenInput.trim();
        if (!trimmed) {
            return;
        }
        localStorage.setItem(JWT_KEY, trimmed);
        setStoredToken(trimmed);
        setTokenInput('');
    };

    const handleClearToken = () => {
        localStorage.removeItem(JWT_KEY);
        setStoredToken('');
    };


    return (
        <div className="app-shell">
            <main className="app-content">
                <h1 className="app-title">FastNotes</h1>

                {!isAuthenticated ? (
                    <section className="token-panel">
                        <h2 className="token-panel__title">Sign in with token</h2>
                        <p className="token-panel__hint">
                            Paste a JWT containing
                            <strong> tasks.read </strong>
                            and
                            <strong> tasks.write </strong>
                            scopes.
                        </p>

                        <form className="token-form" onSubmit={handleSaveToken}>
                            <textarea
                                className="token-input"
                                placeholder="Paste JWT token here"
                                value={tokenInput}
                                onChange={e => setTokenInput(e.target.value)}
                            />

                            <button className="token-save-button" type="submit">
                                Save token
                            </button>
                        </form>
                    </section>
                ) : (
                    <section className="token-state">
                        <div className="token-state__info">
                            <span>Token saved. You can now load tasks.</span>
                            <button
                                className="token-clear-button"
                                onClick={handleClearToken}
                                type="button"
                            >
                                Clear token
                            </button>
                        </div>

                        <TaskList />
                    </section>
                )}
            </main>
        </div>
    );
}

export default App;
