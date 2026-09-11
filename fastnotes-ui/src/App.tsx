import { useMemo, useState } from 'react';
import { TaskList } from './components/TaskList';
import { login } from './api/auth';

const JWT_KEY = 'fastnotes.jwt';

function App() {
    const [userId, setUserId] = useState('dev-user');
    const [canWrite, setCanWrite] = useState(true);
    const [storedToken, setStoredToken] = useState(
        () => localStorage.getItem(JWT_KEY) ?? ''
    );
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(false);

    const isAuthenticated = useMemo(
        () => storedToken.length > 0,
        [storedToken]
    );

    const handleLogin = async (event: React.FormEvent) => {
        event.preventDefault();

        try {
            setIsLoading(true);
            setError('');

            const response = await login({
                userId,
                canWrite
            });

            localStorage.setItem(
                JWT_KEY,
                response.access_token
            );

            setStoredToken(response.access_token);
        } catch {
            setError('Login failed.');
        } finally {
            setIsLoading(false);
        }
    };

    const handleLogout = () => {
        localStorage.removeItem(JWT_KEY);
        setStoredToken('');
    };

    return (
        <div className="app-shell">
            <main className="app-content">
                <h1 className="app-title">FastNotes</h1>

                {!isAuthenticated ? (
                    <section className="token-panel">
                        <h2 className="token-panel__title">
                            Sign in
                        </h2>

                        <form
                            className="token-form"
                            onSubmit={handleLogin}
                        >
                            <input
                                value={userId}
                                onChange={e =>
                                    setUserId(e.target.value)
                                }
                                placeholder="User ID"
                            />

                            <label>
                                <input
                                    type="checkbox"
                                    checked={canWrite}
                                    onChange={e =>
                                        setCanWrite(e.target.checked)
                                    }
                                />
                                Allow write access
                            </label>

                            <button
                                type="submit"
                                disabled={isLoading}
                            >
                                {isLoading
                                    ? 'Signing in...'
                                    : 'Sign in'}
                            </button>

                            {error && <p>{error}</p>}
                        </form>
                    </section>
                ) : (
                    <section className="token-state">
                        <div className="token-state__info">
                            <span>Signed in.</span>

                            <button
                                onClick={handleLogout}
                                type="button"
                            >
                                Sign out
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