import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import Header from "@/components/Header";
import { ChevronLeft, ChevronRight, Star } from "lucide-react";

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

const mockMovies: Movie[] = [
  {
    id: 1,
    title: "Удивительное путешествие доктора Дулиттла",
    image: "https://via.placeholder.com/200x300?text=Dolittle",
    rating: 5,
    year: 2020,
    genres: ["фантастика", "комедия"],
  },
  {
    id: 2,
    title: "Человек-паук: Возвращение домой",
    image: "https://via.placeholder.com/200x300?text=Spider-Man",
    rating: 5,
    year: 2019,
    genres: ["фантастика", "боевик"],
  },
  {
    id: 3,
    title: "Финч",
    image: "https://via.placeholder.com/200x300?text=Finch",
    rating: 4,
    year: 2021,
    genres: ["фантастика", "драма"],
  },
  {
    id: 4,
    title: "Бэтмен",
    image: "https://via.placeholder.com/200x300?text=Batman",
    rating: 5,
    year: 2022,
    genres: ["фантастика", "детектив"],
  },
  {
    id: 5,
    title: "Мстители: Финала",
    image: "https://via.placeholder.com/200x300?text=Avengers",
    rating: 5,
    year: 2019,
    genres: ["фантастика", "боевик"],
  },
  {
    id: 6,
    title: "Гарри Поттер и Дары Смерти",
    image: "https://via.placeholder.com/200x300?text=Harry+Potter",
    rating: 5,
    year: 2011,
    genres: ["фантастика", "приключения"],
  },
];

const upcomingMovies = mockMovies.slice(0, 4);

export default function Index() {
  const [user, setUser] = useState<User | null>(null);
  const [scrollPosition, setScrollPosition] = useState(0);

  useEffect(() => {
    const savedUser = localStorage.getItem("user");
    if (savedUser) {
      setUser(JSON.parse(savedUser));
    }
  }, []);

  const handleLogout = () => {
    localStorage.removeItem("user");
    setUser(null);
  };

  const scroll = (direction: "left" | "right") => {
    const container = document.getElementById("movies-scroll");
    if (container) {
      const scrollAmount = 300;
      const newPosition = scrollPosition + (direction === "left" ? -scrollAmount : scrollAmount);
      container.scrollLeft = newPosition;
      setScrollPosition(newPosition);
    }
  };

  return (
    <div className="min-h-screen bg-background">
      <Header isAuthenticated={!!user} userName={user?.name} onLogout={handleLogout} />

      <main className="max-w-6xl mx-auto px-4 py-8">
        {/* Featured Movie */}
        <section className="mb-12">
          <div className="relative rounded-2xl overflow-hidden h-96 bg-card border border-border">
            <div className="absolute inset-0 bg-gradient-to-r from-background via-transparent to-transparent z-10" />
            
            <div className="absolute inset-0 flex items-center px-8 z-20">
              <div className="max-w-2xl">
                <h1 className="text-5xl font-bold text-foreground mb-4">
                  MORTAL KOMBAT
                </h1>
                <p className="text-lg text-muted-foreground mb-6">
                  GET OVER HERE DALAM HD
                </p>
                <div className="flex gap-4">
                  {user ? (
                    <>
                      <Link
                        to="/film/1"
                        className="px-8 py-3 bg-accent text-accent-foreground rounded-xl font-medium hover:bg-accent/90 transition-colors"
                      >
                        Смотреть
                      </Link>
                      <button className="px-8 py-3 border border-accent text-accent rounded-xl font-medium hover:bg-accent/10 transition-colors">
                        В избранное
                      </button>
                    </>
                  ) : (
                    <>
                      <Link
                        to="/login"
                        className="px-8 py-3 bg-accent text-accent-foreground rounded-xl font-medium hover:bg-accent/90 transition-colors"
                      >
                        Войти для просмотра
                      </Link>
                    </>
                  )}
                </div>
              </div>
            </div>

            <div 
              className="absolute inset-0"
              style={{
                backgroundImage: "url('https://via.placeholder.com/1200x400?text=Featured+Movie')",
                backgroundSize: "cover",
                backgroundPosition: "center",
                opacity: 0.3,
              }}
            />
          </div>
        </section>

        {/* Upcoming Movies */}
        <section className="mb-12">
          <h2 className="text-2xl font-bold text-foreground mb-6">Подборка месяца</h2>
          
          <div className="relative">
            <div 
              id="movies-scroll"
              className="flex gap-4 overflow-x-auto scroll-smooth pb-4"
              style={{ scrollBehavior: "smooth" }}
            >
              {upcomingMovies.map((movie) => (
                <Link
                  key={movie.id}
                  to={`/film/${movie.id}`}
                  className="flex-shrink-0 w-48 group cursor-pointer"
                >
                  <div className="rounded-lg overflow-hidden border border-border hover:border-accent transition-colors">
                    <img 
                      src={movie.image}
                      alt={movie.title}
                      className="w-full h-64 object-cover group-hover:scale-105 transition-transform"
                    />
                  </div>
                </Link>
              ))}
            </div>

            <button
              onClick={() => scroll("left")}
              className="absolute left-0 top-1/2 -translate-y-1/2 -translate-x-4 z-30 p-2 rounded-full bg-card border border-border hover:bg-card/80 transition-colors"
            >
              <ChevronLeft className="w-6 h-6 text-accent" />
            </button>
            <button
              onClick={() => scroll("right")}
              className="absolute right-0 top-1/2 -translate-y-1/2 translate-x-4 z-30 p-2 rounded-full bg-card border border-border hover:bg-card/80 transition-colors"
            >
              <ChevronRight className="w-6 h-6 text-accent" />
            </button>
          </div>
        </section>

        {/* New Movies */}
        <section>
          <h2 className="text-2xl font-bold text-foreground mb-6">Новые фильмы</h2>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {mockMovies.map((movie) => (
              <Link
                key={movie.id}
                to={`/film/${movie.id}`}
                className="group bg-card rounded-xl border border-border overflow-hidden hover:border-accent transition-colors"
              >
                <div className="flex gap-4">
                  <div className="flex-shrink-0 w-32 h-40 overflow-hidden">
                    <img 
                      src={movie.image}
                      alt={movie.title}
                      className="w-full h-full object-cover group-hover:scale-105 transition-transform"
                    />
                  </div>
                  
                  <div className="flex-1 p-4 flex flex-col justify-between">
                    <div>
                      <h3 className="text-lg font-semibold text-foreground mb-2 line-clamp-2">
                        {movie.title}
                      </h3>
                      <p className="text-sm text-muted-foreground mb-3">
                        {movie.year}
                      </p>
                      <div className="flex flex-wrap gap-2 mb-3">
                        {movie.genres.map((genre) => (
                          <span
                            key={genre}
                            className="px-3 py-1 bg-primary text-primary-foreground rounded-full text-xs"
                          >
                            {genre}
                          </span>
                        ))}
                      </div>
                    </div>
                    
                    <div className="flex items-center gap-1">
                      {Array.from({ length: movie.rating }).map((_, i) => (
                        <Star key={i} className="w-4 h-4 fill-accent text-accent" />
                      ))}
                      {Array.from({ length: 5 - movie.rating }).map((_, i) => (
                        <Star key={`empty-${i}`} className="w-4 h-4 text-muted" />
                      ))}
                    </div>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        </section>
      </main>
    </div>
  );
}
