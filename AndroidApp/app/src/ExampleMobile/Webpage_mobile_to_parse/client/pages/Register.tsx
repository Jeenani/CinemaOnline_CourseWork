import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Film } from "lucide-react";

export default function Register() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    name: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [error, setError] = useState("");

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
    setError("");
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.name || !formData.email || !formData.password || !formData.confirmPassword) {
      setError("Пожалуйста, заполните все поля");
      return;
    }

    if (formData.password !== formData.confirmPassword) {
      setError("Пароли не совпадают");
      return;
    }

    if (formData.password.length < 6) {
      setError("Пароль должен быть не менее 6 символов");
      return;
    }

    // Mock registration
    localStorage.setItem("user", JSON.stringify({
      email: formData.email,
      name: formData.name,
    }));
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
            Регистрация
          </h2>

          <form onSubmit={handleSubmit} className="space-y-5">
            {error && (
              <div className="p-3 bg-destructive/10 border border-destructive rounded-lg text-destructive text-sm">
                {error}
              </div>
            )}

            <div>
              <label className="block text-sm font-medium text-foreground mb-3">
                Имя
              </label>
              <input
                type="text"
                name="name"
                value={formData.name}
                onChange={handleChange}
                placeholder="Ваше имя"
                className="w-full px-4 py-3 bg-primary text-primary-foreground rounded-xl border-0 focus:outline-none focus:ring-2 focus:ring-accent placeholder:text-primary-foreground/50"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-3">
                E-MAIL
              </label>
              <input
                type="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
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
                name="password"
                value={formData.password}
                onChange={handleChange}
                placeholder="••••••••"
                className="w-full px-4 py-3 bg-primary text-primary-foreground rounded-xl border-0 focus:outline-none focus:ring-2 focus:ring-accent placeholder:text-primary-foreground/50"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-3">
                Подтвердите пароль
              </label>
              <input
                type="password"
                name="confirmPassword"
                value={formData.confirmPassword}
                onChange={handleChange}
                placeholder="••••••••"
                className="w-full px-4 py-3 bg-primary text-primary-foreground rounded-xl border-0 focus:outline-none focus:ring-2 focus:ring-accent placeholder:text-primary-foreground/50"
              />
            </div>

            <button
              type="submit"
              className="w-full px-4 py-3 bg-primary text-primary-foreground rounded-xl font-medium hover:bg-primary/90 transition-colors"
            >
              Зарегистрироваться
            </button>
          </form>

          <div className="mt-6 pt-6 border-t border-border text-center">
            <p className="text-sm text-muted-foreground">
              Уже есть аккаунт?{" "}
              <Link to="/login" className="text-accent hover:text-accent/80">
                Войти
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
