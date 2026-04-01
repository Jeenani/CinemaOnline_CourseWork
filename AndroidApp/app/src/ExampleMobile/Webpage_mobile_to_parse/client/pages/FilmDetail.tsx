import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import Header from "@/components/Header";
import { ChevronLeft, ChevronRight, Star, Play, BookmarkPlus, Share2 } from "lucide-react";

interface User {
  name: string;
  email: string;
}

const mockMovie = {
  id: 1,
  title: "Гарри Поттер и Дары Смерти",
  subtitle: "Часть 1 (2010)",
  image: "https://via.placeholder.com/200x300?text=Harry+Potter",
  rating: 5,
  description:
    "Гарри Поттера ждет самое странное испытание в жизни — смертельная схватка с Волан-Мортом. Жадь помощи от кого-то, Гарри одиноток, как никогда, Друзья и враги предстают в совершенном новом свете.",
  year: 2010,
  genres: ["фантастика", "приключения", "подростки"],
  director: "Дэвид Йейтс",
  cast: ["Дэниэл Рэдклифф", "Руперт Гринт", "Эмма Уотсон"],
  duration: "146 минут",
  country: "США, Британия",
};

const relatedMovies = [
  {
    id: 1,
    title: "Гарри Поттер и Философский камень",
    image: "https://via.placeholder.com/150x225?text=HP1",
  },
  {
    id: 2,
    title: "Гарри Поттер и Тайная комната",
    image: "https://via.placeholder.com/150x225?text=HP2",
  },
  {
    id: 3,
    title: "Гарри Поттер и Узник Азкабана",
    image: "https://via.placeholder.com/150x225?text=HP3",
  },
  {
    id: 4,
    title: "Гарри Поттер и Кубок огня",
    image: "https://via.placeholder.com/150x225?text=HP4",
  },
];

const comments = [
  {
    id: 1,
    author: "Наша память",
    rating: 5,
    text: "Есть фильмы-сыщики. А есть фильмы-каталоги. Первый видное - на жизнь вспоминается. Вторые — помогают выбрать жизнь. Вторые — помогают выбрать одной! «ГАРРИ ПОТТЕР И ДАРЫ СМЕРТИ» — это самый!",
  },
  {
    id: 2,
    author: "Георг Ванадилов",
    rating: 5,
    text: "Есть фильмы-сыщики. А есть фильмы-каталоги. Первый видное - на жизнь вспоминается. Вторые — помогают выбрать жизнь. Вторые — помогают выбрать одной! «ГАРРИ ПОТТЕР И ДАРЫ СМЕРТИ» — это самый!",
  },
];

export default function FilmDetail() {
  const { id } = useParams();
  const [user, setUser] = useState<User | null>(null);
  const [scrollPosition, setScrollPosition] = useState(0);

  useEffect(() => {
    const savedUser = localStorage.getItem("user");
    if (savedUser) {
      setUser(JSON.parse(savedUser));
    }
    window.scrollTo(0, 0);
  }, [id]);

  const handleLogout = () => {
    localStorage.removeItem("user");
    setUser(null);
  };

  const scroll = (direction: "left" | "right") => {
    const container = document.getElementById("related-scroll");
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
        {/* Movie Header */}
        <div className="flex flex-col md:flex-row gap-8 mb-12">
          {/* Poster */}
          <div className="flex-shrink-0 w-full md:w-64">
            <img
              src={mockMovie.image}
              alt={mockMovie.title}
              className="w-full rounded-xl border border-border"
            />
          </div>

          {/* Details */}
          <div className="flex-1">
            <div className="mb-4">
              <h1 className="text-4xl font-bold text-foreground mb-2">
                {mockMovie.title}
              </h1>
              <p className="text-lg text-muted-foreground">
                {mockMovie.subtitle}
              </p>
            </div>

            {/* Rating */}
            <div className="flex items-center gap-2 mb-6">
              {Array.from({ length: mockMovie.rating }).map((_, i) => (
                <Star key={i} className="w-6 h-6 fill-accent text-accent" />
              ))}
              {Array.from({ length: 5 - mockMovie.rating }).map((_, i) => (
                <Star key={`empty-${i}`} className="w-6 h-6 text-muted" />
              ))}
            </div>

            {/* Info */}
            <div className="grid grid-cols-2 gap-4 mb-8 p-4 bg-card rounded-lg border border-border">
              <div>
                <p className="text-sm text-muted-foreground mb-1">Жанр</p>
                <p className="text-foreground">
                  {mockMovie.genres.join(", ")}
                </p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground mb-1">Год</p>
                <p className="text-foreground">{mockMovie.year}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground mb-1">Режиссер</p>
                <p className="text-foreground">{mockMovie.director}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground mb-1">Длительность</p>
                <p className="text-foreground">{mockMovie.duration}</p>
              </div>
            </div>

            {/* Description */}
            <p className="text-foreground mb-8 leading-relaxed">
              {mockMovie.description}
            </p>

            {/* Buttons */}
            <div className="flex flex-wrap gap-3">
              {user ? (
                <>
                  <button className="flex items-center gap-2 px-6 py-3 bg-accent text-accent-foreground rounded-xl font-medium hover:bg-accent/90 transition-colors">
                    <Play className="w-5 h-5" />
                    Смотреть
                  </button>
                  <button className="flex items-center gap-2 px-6 py-3 border border-accent text-accent rounded-xl font-medium hover:bg-accent/10 transition-colors">
                    <BookmarkPlus className="w-5 h-5" />
                    В избранное
                  </button>
                  <button className="flex items-center gap-2 px-6 py-3 border border-border text-foreground rounded-xl font-medium hover:bg-card transition-colors">
                    <Share2 className="w-5 h-5" />
                    Поделиться
                  </button>
                </>
              ) : (
                <Link
                  to="/login"
                  className="flex items-center gap-2 px-6 py-3 bg-accent text-accent-foreground rounded-xl font-medium hover:bg-accent/90 transition-colors"
                >
                  <Play className="w-5 h-5" />
                  Войти для просмотра
                </Link>
              )}
            </div>
          </div>
        </div>

        {/* Related Movies */}
        <section className="mb-12">
          <h2 className="text-2xl font-bold text-foreground mb-6">Другие части</h2>

          <div className="relative">
            <div
              id="related-scroll"
              className="flex gap-4 overflow-x-auto scroll-smooth pb-4"
              style={{ scrollBehavior: "smooth" }}
            >
              {relatedMovies.map((movie) => (
                <Link
                  key={movie.id}
                  to={`/film/${movie.id}`}
                  className="flex-shrink-0 w-40 group cursor-pointer"
                >
                  <div className="rounded-lg overflow-hidden border border-border hover:border-accent transition-colors">
                    <img
                      src={movie.image}
                      alt={movie.title}
                      className="w-full h-56 object-cover group-hover:scale-105 transition-transform"
                    />
                  </div>
                  <p className="mt-2 text-sm text-foreground line-clamp-2">
                    {movie.title}
                  </p>
                </Link>
              ))}
            </div>

            <button
              onClick={() => scroll("left")}
              className="absolute left-0 top-20 -translate-x-4 z-30 p-2 rounded-full bg-card border border-border hover:bg-card/80 transition-colors"
            >
              <ChevronLeft className="w-6 h-6 text-accent" />
            </button>
            <button
              onClick={() => scroll("right")}
              className="absolute right-0 top-20 translate-x-4 z-30 p-2 rounded-full bg-card border border-border hover:bg-card/80 transition-colors"
            >
              <ChevronRight className="w-6 h-6 text-accent" />
            </button>
          </div>
        </section>

        {/* Video Player */}
        {user && (
          <section className="mb-12">
            <div className="bg-card rounded-xl border border-border overflow-hidden">
              <div className="aspect-video bg-black flex items-center justify-center">
                <Play className="w-20 h-20 text-accent opacity-50" />
              </div>
              <div className="p-4 flex items-center gap-2">
                <button className="p-2 hover:bg-secondary rounded-lg transition-colors">
                  <Play className="w-5 h-5 text-accent" />
                </button>
                <div className="flex-1 h-1 bg-muted rounded-full">
                  <div className="h-full w-1/3 bg-accent rounded-full"></div>
                </div>
                <span className="text-xs text-muted-foreground">0:00 / 0:00</span>
              </div>
            </div>
          </section>
        )}

        {/* Comments Section */}
        <section>
          <h2 className="text-2xl font-bold text-foreground mb-6">
            {user ? "Оставить комментарий:" : "Комментарии"}
          </h2>

          {user && (
            <div className="mb-8 p-4 bg-card rounded-xl border border-border">
              <div className="flex gap-4 mb-4">
                <div className="w-10 h-10 rounded-full bg-accent flex items-center justify-center flex-shrink-0">
                  <span className="text-accent-foreground font-bold">Н</span>
                </div>
                <div className="flex-1">
                  <p className="text-foreground font-medium mb-2">Напишите что-нибудь...</p>
                  <div className="flex gap-1 mb-4">
                    {[...Array(5)].map((_, i) => (
                      <button
                        key={i}
                        className="hover:scale-110 transition-transform"
                      >
                        <Star className="w-5 h-5 text-muted hover:fill-accent hover:text-accent" />
                      </button>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          )}

          <div className="space-y-4">
            {comments.map((comment) => (
              <div
                key={comment.id}
                className="p-4 bg-card rounded-xl border border-border"
              >
                <div className="flex gap-4">
                  <div className="w-10 h-10 rounded-full bg-accent flex items-center justify-center flex-shrink-0">
                    <span className="text-accent-foreground font-bold">
                      {comment.author[0]}
                    </span>
                  </div>
                  <div className="flex-1">
                    <p className="text-foreground font-medium">
                      {comment.author}
                    </p>
                    <div className="flex items-center gap-1 mb-2">
                      {Array.from({ length: comment.rating }).map((_, i) => (
                        <Star
                          key={i}
                          className="w-4 h-4 fill-accent text-accent"
                        />
                      ))}
                      {Array.from({ length: 5 - comment.rating }).map(
                        (_, i) => (
                          <Star
                            key={`empty-${i}`}
                            className="w-4 h-4 text-muted"
                          />
                        )
                      )}
                    </div>
                    <p className="text-muted-foreground text-sm">
                      {comment.text}
                    </p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </section>
      </main>
    </div>
  );
}
