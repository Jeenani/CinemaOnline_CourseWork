import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import Header from "@/components/Header";
import { Star, Search as SearchIcon, ChevronDown } from "lucide-react";

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
];

const genres = [
  "Все жанры",
  "фантастика",
  "боевик",
  "комедия",
  "драма",
  "детектив",
  "приключения",
];

type SortOption = "Сначала новые" | "Сначала старые" | "По рейтингу";

export default function Search() {
  const [user, setUser] = useState<User | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedGenre, setSelectedGenre] = useState("Все жанры");
  const [yearFrom, setYearFrom] = useState("");
  const [yearTo, setYearTo] = useState("");
  const [sortBy, setSortBy] = useState<SortOption>("Сначала новые");
  const [results, setResults] = useState<Movie[]>(allMovies);

  useEffect(() => {
    const savedUser = localStorage.getItem("user");
    if (savedUser) {
      setUser(JSON.parse(savedUser));
    }
  }, []);

  useEffect(() => {
    let filtered = allMovies.filter((movie) => {
      const matchesSearch = movie.title
        .toLowerCase()
        .includes(searchQuery.toLowerCase());

      const matchesGenre =
        selectedGenre === "Все жанры" ||
        movie.genres.includes(selectedGenre);

      const matchesYearFrom = !yearFrom || movie.year >= parseInt(yearFrom);
      const matchesYearTo = !yearTo || movie.year <= parseInt(yearTo);

      return (
        matchesSearch &&
        matchesGenre &&
        matchesYearFrom &&
        matchesYearTo
      );
    });

    // Sort
    if (sortBy === "Сначала новые") {
      filtered.sort((a, b) => b.year - a.year);
    } else if (sortBy === "Сначала старые") {
      filtered.sort((a, b) => a.year - b.year);
    } else if (sortBy === "По рейтингу") {
      filtered.sort((a, b) => b.rating - a.rating);
    }

    setResults(filtered);
  }, [searchQuery, selectedGenre, yearFrom, yearTo, sortBy]);

  const handleLogout = () => {
    localStorage.removeItem("user");
    setUser(null);
  };

  return (
    <div className="min-h-screen bg-background">
      <Header isAuthenticated={!!user} userName={user?.name} onLogout={handleLogout} />

      <main className="max-w-6xl mx-auto px-4 py-8">
        {/* Filters */}
        <div className="mb-8 p-6 bg-card rounded-2xl border border-border">
          <h2 className="text-lg font-bold text-foreground mb-6">Поиск по названию</h2>

          <div className="space-y-6">
            {/* Search */}
            <div>
              <label className="block text-sm font-medium text-foreground mb-3">
                Поиск по названию
              </label>
              <div className="relative">
                <SearchIcon className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-muted-foreground" />
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Введите название фильма..."
                  className="w-full pl-12 pr-4 py-3 bg-primary text-primary-foreground rounded-xl border-0 focus:outline-none focus:ring-2 focus:ring-accent placeholder:text-primary-foreground/50"
                />
              </div>
            </div>

            {/* Genre */}
            <div>
              <label className="block text-sm font-medium text-foreground mb-3">
                Жанр
              </label>
              <div className="relative inline-block w-full">
                <select
                  value={selectedGenre}
                  onChange={(e) => setSelectedGenre(e.target.value)}
                  className="w-full appearance-none px-4 py-3 bg-primary text-primary-foreground rounded-xl border-0 focus:outline-none focus:ring-2 focus:ring-accent cursor-pointer"
                >
                  {genres.map((genre) => (
                    <option key={genre} value={genre}>
                      {genre}
                    </option>
                  ))}
                </select>
                <ChevronDown className="absolute right-4 top-1/2 -translate-y-1/2 w-5 h-5 pointer-events-none text-primary-foreground" />
              </div>
            </div>

            {/* Year Range */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-foreground mb-3">
                  Год от
                </label>
                <input
                  type="text"
                  value={yearFrom}
                  onChange={(e) => setYearFrom(e.target.value)}
                  placeholder="2000"
                  className="w-full px-4 py-3 bg-primary text-primary-foreground rounded-xl border-0 focus:outline-none focus:ring-2 focus:ring-accent placeholder:text-primary-foreground/50"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-foreground mb-3">
                  Год до
                </label>
                <input
                  type="text"
                  value={yearTo}
                  onChange={(e) => setYearTo(e.target.value)}
                  placeholder="2024"
                  className="w-full px-4 py-3 bg-primary text-primary-foreground rounded-xl border-0 focus:outline-none focus:ring-2 focus:ring-accent placeholder:text-primary-foreground/50"
                />
              </div>
            </div>

            {/* Sort */}
            <div>
              <label className="block text-sm font-medium text-foreground mb-3">
                Сортировка
              </label>
              <div className="relative inline-block w-full">
                <select
                  value={sortBy}
                  onChange={(e) => setSortBy(e.target.value as SortOption)}
                  className="w-full appearance-none px-4 py-3 bg-primary text-primary-foreground rounded-xl border-0 focus:outline-none focus:ring-2 focus:ring-accent cursor-pointer"
                >
                  <option>Сначала новые</option>
                  <option>Сначала старые</option>
                  <option>По рейтингу</option>
                </select>
                <ChevronDown className="absolute right-4 top-1/2 -translate-y-1/2 w-5 h-5 pointer-events-none text-primary-foreground" />
              </div>
            </div>

            {/* Buttons */}
            <div className="flex gap-3">
              <button
                onClick={() => {
                  setSearchQuery("");
                  setSelectedGenre("Все жанры");
                  setYearFrom("");
                  setYearTo("");
                  setSortBy("Сначала новые");
                }}
                className="flex-1 px-4 py-3 bg-accent text-accent-foreground rounded-xl font-medium hover:bg-accent/90 transition-colors"
              >
                Найти фильмы
              </button>
              <button
                onClick={() => {
                  setSearchQuery("");
                  setSelectedGenre("Все жанры");
                  setYearFrom("");
                  setYearTo("");
                  setSortBy("Сначала новые");
                }}
                className="flex-1 px-4 py-3 border border-muted text-foreground rounded-xl font-medium hover:bg-card transition-colors"
              >
                Сбросить
              </button>
            </div>
          </div>
        </div>

        {/* Results */}
        <section>
          <h2 className="text-2xl font-bold text-foreground mb-6">
            Найдено фильмов: {results.length}
          </h2>

          {results.length > 0 ? (
            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
              {results.map((movie) => (
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
                  </div>
                </Link>
              ))}
            </div>
          ) : (
            <div className="text-center py-12">
              <p className="text-muted-foreground mb-4">Фильмы не найдены</p>
              <p className="text-sm text-muted-foreground">
                Попробуйте изменить параметры поиска
              </p>
            </div>
          )}
        </section>
      </main>
    </div>
  );
}
