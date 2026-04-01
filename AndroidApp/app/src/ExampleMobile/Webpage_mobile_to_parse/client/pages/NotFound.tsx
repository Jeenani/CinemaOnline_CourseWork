import { useLocation, Link } from "react-router-dom";
import { useEffect } from "react";
import Header from "@/components/Header";

const NotFound = () => {
  const location = useLocation();

  useEffect(() => {
    console.error(
      "404 Error: User attempted to access non-existent route:",
      location.pathname,
    );
  }, [location.pathname]);

  return (
    <div className="min-h-screen bg-background">
      <Header />
      <div className="min-h-[calc(100vh-100px)] flex items-center justify-center px-4">
        <div className="text-center max-w-2xl">
          <h1 className="text-6xl font-bold text-foreground mb-4">404</h1>
          <p className="text-2xl text-muted-foreground mb-4">
            Страница не найдена
          </p>
          <p className="text-foreground mb-8">
            Запрошенная страница не существует или была удалена.
          </p>
          <Link
            to="/"
            className="inline-block px-8 py-3 bg-accent text-accent-foreground rounded-xl font-medium hover:bg-accent/90 transition-colors"
          >
            Вернуться на главную
          </Link>
        </div>
      </div>
    </div>
  );
};

export default NotFound;
