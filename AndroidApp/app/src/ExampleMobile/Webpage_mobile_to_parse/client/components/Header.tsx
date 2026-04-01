import { Link, useLocation } from "react-router-dom";
import { Film, User, LogOut } from "lucide-react";

interface HeaderProps {
  isAuthenticated?: boolean;
  userName?: string;
  onLogout?: () => void;
}

export default function Header({
  isAuthenticated = false,
  userName,
  onLogout,
}: HeaderProps) {
  const location = useLocation();

  const navLinks = [
    { path: "/", label: "Главная" },
    { path: "/films", label: "Фильмы" },
    { path: "/search", label: "Поиск" },
  ];

  const isActive = (path: string) => location.pathname === path;

  return (
    <header className="border-b border-border bg-background sticky top-0 z-50">
      <div className="max-w-6xl mx-auto px-4 py-4">
        <div className="flex items-center justify-between">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-2">
            <Film className="w-6 h-6 text-accent" />
            <span className="text-xl font-bold text-foreground">
              CINEMA ONLINE
            </span>
          </Link>

          {/* Navigation */}
          <nav className="hidden md:flex items-center gap-8">
            {navLinks.map((link) => (
              <Link
                key={link.path}
                to={link.path}
                className={`text-sm font-medium transition-colors ${
                  isActive(link.path)
                    ? "text-accent"
                    : "text-foreground hover:text-accent"
                }`}
              >
                {link.label}
              </Link>
            ))}
          </nav>

          {/* User Section */}
          <div className="flex items-center gap-4">
            {isAuthenticated ? (
              <>
                <Link
                  to="/profile"
                  className="flex items-center gap-2 text-sm text-foreground hover:text-accent transition-colors"
                >
                  <User className="w-5 h-5" />
                  <span className="hidden sm:inline">{userName || "Пользователь"}</span>
                </Link>
                <button
                  onClick={onLogout}
                  className="text-sm text-muted-foreground hover:text-accent transition-colors"
                  title="Выход"
                >
                  <LogOut className="w-5 h-5" />
                </button>
              </>
            ) : (
              <div className="flex items-center gap-2">
                <Link
                  to="/login"
                  className="text-sm text-foreground hover:text-accent transition-colors"
                >
                  Вход
                </Link>
                <span className="text-muted-foreground">/</span>
                <Link
                  to="/register"
                  className="text-sm text-foreground hover:text-accent transition-colors"
                >
                  Регистрация
                </Link>
              </div>
            )}
          </div>
        </div>

        {/* Mobile Navigation */}
        <nav className="md:hidden flex items-center justify-center gap-6 mt-4 pt-4 border-t border-border">
          {navLinks.map((link) => (
            <Link
              key={link.path}
              to={link.path}
              className={`text-xs font-medium transition-colors ${
                isActive(link.path)
                  ? "text-accent"
                  : "text-foreground hover:text-accent"
              }`}
            >
              {link.label}
            </Link>
          ))}
        </nav>
      </div>
    </header>
  );
}
