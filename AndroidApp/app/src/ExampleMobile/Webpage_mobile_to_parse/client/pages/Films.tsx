import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import Header from "@/components/Header";
import { Star } from "lucide-react";

interface User {
  name: string;
  email: string;
}

interface Movie {
  id: number;
  title: string;
  image: string;
  rating: number;
  year: number;
  genres: string[];
}

const allMovies: Movie[] = [
  {
    id: 1,
    title: "Удивительное путешествие доктора Дулиттла",
    image: "https://via.placeholder.com/150x225?text=Dolittle",
    rating: 5,
    year: 2020,
    genres: ["фантастика", "комедия"],
  },
  {
    id: 2,
    title: "Человек-паук: Возвращение домой",
    image: "https://via.placeholder.com/150x225?text=Spider-Man",
    rating: 5,
    year: 2019,
    genres: ["фантастика", "боевик"],
  },
  {
    id: 3,
    title: "Финч",
    image: "https://via.placeholder.com/150x225?text=Finch",
    rating: 4,
    year: 2021,
    genres: ["фантастика", "драма"],
  },
  {
    id: 4,
    title: "Бэтмен",
    image: "https://via.placeholder.com/150x225?text=Batman",
    rating: 5,
    year: 2022,
    genres: ["фантастика", "детектив"],
  },
  {
    id: 5,
    title: "Мстители: Финала",
    image: "https://via.placeholder.com/150x225?text=Avengers",
    rating: 5,
    year: 2019,
    genres: ["фантастика", "боевик"],
  },
  {
    id: 6,
    title: "Гарри Поттер и Дары Смерти",
    image: "https://via.placeholder.com/150x225?text=Harry+Potter",
    rating: 5,
    year: 2011,
    genres: ["фантастика", "приключения"],
  },
  {
    id: 7,
    title: "Шан-чи и легенда о десяти кольцах",
    image: "https://via.placeholder.com/150x225?text=ShangChi",
    rating: 4,
    year: 2021,
    genres: ["фантастика", "боевик"],
  },
  {
    id: 8,
    title: "Чёрная вдова",
    image: "https://via.placeholder.com/150x225?text=BlackWidow",
    rating: 5,
    year: 2021,
    genres: ["фантастика", "боевик"],
  },
];

export default function Films() {
  const [user, setUser] = useState<User | null>(null);
  const [displayMovies, setDisplayMovies] = useState<Movie[]>(allMovies);

  useEffect(() => {
    const savedUser = localStorage.getItem("user");
    if (savedUser) {
      setUser(JSON.parse(savedUser));
    }
    window.scrollTo(0, 0);
  }, []);

  const handleLogout = () => {
    localStorage.removeItem("user");
    setUser(null);
  };

  return (
    <div className="min-h-screen bg-background">
      <Header isAuthenticated={!!user} userName={user?.name} onLogout={handleLogout} />

      <main className="max-w-6xl mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold text-foreground mb-8">Фильмы</h1>

        {displayMovies.length > 0 ? (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
            {displayMovies.map((movie) => (
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
                <p className="text-xs text-muted-foreground mb-2">
                  {movie.year}
                </p>
                <div className="flex items-center gap-1">
                  {Array.from({ length: movie.rating }).map((_, i) => (
                    <Star key={i} className="w-3 h-3 fill-accent text-accent" />
                  ))}
                  {Array.from({ length: 5 - movie.rating }).map((_, i) => (
                    <Star key={`empty-${i}`} className="w-3 h-3 text-muted" />
                  ))}
                </div>
              </Link>
            ))}
          </div>
        ) : (
          <div className="text-center py-12">
            <p className="text-muted-foreground">Фильмы не найдены</p>
          </div>
        )}
      </main>
    </div>
  );
}
