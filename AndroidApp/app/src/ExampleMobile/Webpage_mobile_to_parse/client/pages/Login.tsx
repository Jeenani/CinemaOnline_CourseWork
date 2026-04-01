import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Film } from "lucide-react";

export default function Login() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!email || !password) {
      setError("Пожалуйста, заполните все поля");
      return;
    }
    // Mock login - redirect to home
    localStorage.setItem("user", JSON.stringify({ email, name: email.split("@")[0] }));
    navigate("/");
  };

  return (
    <div className="min-h-screen bg-background flex items-center justify-center px-4 py-8">
      <div className="w-full max-w-md">
        <div className="text-center mb-12">
          <div className="flex items-center justify-center gap-3 mb-4">
            <Film className="w-8 h-8 text-accent" />
            <h1 className="text-2xl font-bold text-foreground">CINEMA ONLINE</h1>
          </div>
        </div>

        <div className="bg-card rounded-2xl p-8 border border-border">
          <h2 className="text-3xl font-light text-center text-foreground mb-8">
            Вход в аккаунт
          </h2>

          <form onSubmit={handleSubmit} className="space-y-6">
            {error && (
              <div className="p-3 bg-destructive/10 border border-destructive rounded-lg text-destructive text-sm">
                {error}
              </div>
            )}

            <div>
              <label className="block text-sm font-medium text-foreground mb-3">
                E-MAIL
              </label>
              <input
                type="email"
                value={email}
                onChange={(e) => {
                  setEmail(e.target.value);
                  setError("");
                }}
                placeholder="your@email.com"
                className="w-full px-4 py-3 bg-primary text-primary-foreground rounded-xl border-0 focus:outline-none focus:ring-2 focus:ring-accent placeholder:text-primary-foreground/50"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-3">
                Пароль
              </label>
              <input
                type="password"
                value={password}
                onChange={(e) => {
                  setPassword(e.target.value);
                  setError("");
                }}
                placeholder="••••••••"
                className="w-full px-4 py-3 bg-primary text-primary-foreground rounded-xl border-0 focus:outline-none focus:ring-2 focus:ring-accent placeholder:text-primary-foreground/50"
              />
            </div>

            <button
              type="submit"
              className="w-full px-4 py-3 bg-primary text-primary-foreground rounded-xl font-medium hover:bg-primary/90 transition-colors"
            >
              Войти
            </button>
          </form>

          <div className="mt-6 pt-6 border-t border-border text-center">
            <p className="text-sm text-muted-foreground">
              Нет аккаунта?{" "}
              <Link to="/register" className="text-accent hover:text-accent/80">
                Зарегистрироваться
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
