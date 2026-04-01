import { useEffect, useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import Header from "@/components/Header";
import { Star, ChevronRight, Bookmark, Settings, LogOut } from "lucide-react";

interface User {
  name: string;
  email: string;
}

interface Favorite {
  id: number;
  title: string;
  image: string;
  rating: number;
}

const favoriteMovies: Favorite[] = [
  {
    id: 1,
    title: "Шан-чи и легенда о десяти кольцах",
    image: "https://via.placeholder.com/150x225?text=ShangChi",
    rating: 5,
  },
  {
    id: 2,
    title: "Чёрная вдова",
    image: "https://via.placeholder.com/150x225?text=BlackWidow",
    rating: 5,
  },
  {
    id: 3,
    title: "Человек-паук: Возвращение домой",
    image: "https://via.placeholder.com/150x225?text=SpiderMan",
    rating: 5,
  },
  {
    id: 4,
    title: "Вечные",
    image: "https://via.placeholder.com/150x225?text=Eternals",
    rating: 4,
  },
  {
    id: 5,
    title: "Гарри Поттер и Дары Смерти",
    image: "https://via.placeholder.com/150x225?text=HarryPotter",
    rating: 5,
  },
];

export default function Profile() {
  const navigate = useNavigate();
  const [user, setUser] = useState<User | null>(null);
  const [isPremium, setIsPremium] = useState(false);

  useEffect(() => {
    const savedUser = localStorage.getItem("user");
    if (!savedUser) {
      navigate("/login");
      return;
    }
    setUser(JSON.parse(savedUser));
  }, [navigate]);

  const handleLogout = () => {
    localStorage.removeItem("user");
    navigate("/login");
  };

  if (!user) {
    return null;
  }

  return (
    <div className="min-h-screen bg-background">
      <Header isAuthenticated={true} userName={user.name} onLogout={handleLogout} />

      <main className="max-w-6xl mx-auto px-4 py-8">
        {/* Profile Header */}
        <div className="mb-12">
          <div className="flex items-center gap-6 mb-8">
            <div className="w-24 h-24 rounded-full bg-accent flex items-center justify-center flex-shrink-0">
              <span className="text-3xl font-bold text-accent-foreground">
                {user.name[0].toUpperCase()}
              </span>
            </div>
            <div className="flex-1">
              <h1 className="text-3xl font-bold text-foreground mb-2">
                {user.name}
              </h1>
              <p className="text-muted-foreground mb-4">{user.email}</p>
              <button
                onClick={() => navigate("/password")}
                className="text-accent hover:text-accent/80 transition-colors"
              >
                Сменить пароль
              </button>
            </div>
          </div>

          {/* Subscription Status */}
          <div className="p-6 bg-card rounded-xl border border-border">
            <h2 className="text-lg font-bold text-foreground mb-4">
              Статус подписки
            </h2>
            {isPremium ? (
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-foreground font-medium mb-1">Premium подписка</p>
                  <p className="text-sm text-muted-foreground">
                    Активна до 31 декабря 2024
                  </p>
                </div>
                <span className="px-4 py-2 bg-accent text-accent-foreground rounded-lg text-sm font-medium">
                  Активна
                </span>
              </div>
            ) : (
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-foreground font-medium mb-1">
                    Бесплатная подписка
                  </p>
                  <p className="text-sm text-muted-foreground">
                    Получите доступ ко всем фильмам с Premium
                  </p>
                </div>
                <Link
                  to="/subscription"
                  className="px-4 py-2 bg-accent text-accent-foreground rounded-lg text-sm font-medium hover:bg-accent/90 transition-colors"
                >
                  Обновить
                </Link>
              </div>
            )}
          </div>
        </div>

        {/* Favorites */}
        <section className="mb-12">
          <h2 className="text-2xl font-bold text-foreground mb-6">Любимое</h2>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
            {favoriteMovies.map((movie) => (
              <Link
                key={movie.id}
                to={`/film/${movie.id}`}
                className="group cursor-pointer"
              >
                <div className="rounded-lg overflow-hidden border border-border hover:border-accent transition-colors mb-3">
                  <img
                    src={movie.image}
                    alt={movie.title}
                    className="w-full h-56 object-cover group-hover:scale-105 transition-transform"
                  />
                </div>
                <p className="text-sm text-foreground line-clamp-2 mb-2">
                  {movie.title}
                </p>
                <div className="flex items-center gap-1">
                  {Array.from({ length: movie.rating }).map((_, i) => (
                    <Star key={i} className="w-3 h-3 fill-accent text-accent" />
                  ))}
                </div>
              </Link>
            ))}
          </div>
        </section>

        {/* Account Settings */}
        <section className="mb-12">
          <h2 className="text-2xl font-bold text-foreground mb-6">
            Настройки аккаунта
          </h2>
          <div className="space-y-3">
            <Link
              to="/settings"
              className="flex items-center justify-between p-4 bg-card rounded-xl border border-border hover:border-accent transition-colors"
            >
              <div className="flex items-center gap-3">
                <Settings className="w-5 h-5 text-accent" />
                <span className="text-foreground font-medium">Общие настройки</span>
              </div>
              <ChevronRight className="w-5 h-5 text-muted-foreground" />
            </Link>
            <button
              onClick={handleLogout}
              className="w-full flex items-center justify-between p-4 bg-card rounded-xl border border-border hover:border-accent transition-colors text-left"
            >
              <div className="flex items-center gap-3">
                <LogOut className="w-5 h-5 text-destructive" />
                <span className="text-foreground font-medium">Выход</span>
              </div>
              <ChevronRight className="w-5 h-5 text-muted-foreground" />
            </button>
          </div>
        </section>
      </main>
    </div>
  );
}
