import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import Header from "@/components/Header";
import { Check, ChevronLeft } from "lucide-react";

interface User {
  name: string;
  email: string;
}

interface Plan {
  id: string;
  name: string;
  price: string;
  duration: string;
  features: string[];
  featured?: boolean;
}

const plans: Plan[] = [
  {
    id: "premium",
    name: "Premium",
    price: "9.99",
    duration: "НА 30 ДНЕЙ",
    featured: true,
    features: [
      "Все премиум фильмы",
      "HD качество",
      "Без рекламы",
      "Поддержка 24/7",
    ],
  },
  {
    id: "basic",
    name: "Basic",
    price: "5.99",
    duration: "НА 30 ДНЕЙ",
    features: [
      "Все премиум фильмы",
      "HD качество",
    ],
  },
];

export default function Subscription() {
  const navigate = useNavigate();
  const [user, setUser] = useState<User | null>(null);

  useEffect(() => {
    const savedUser = localStorage.getItem("user");
    if (!savedUser) {
      navigate("/login");
      return;
    }
    setUser(JSON.parse(savedUser));
  }, [navigate]);

  if (!user) {
    return null;
  }

  const handleLogout = () => {
    localStorage.removeItem("user");
    navigate("/login");
  };

  return (
    <div className="min-h-screen bg-background">
      <Header isAuthenticated={true} userName={user.name} onLogout={handleLogout} />

      <main className="max-w-6xl mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-12 text-center">
          <button
            onClick={() => navigate(-1)}
            className="mb-6 inline-flex items-center gap-2 text-accent hover:text-accent/80 transition-colors"
          >
            <ChevronLeft className="w-5 h-5" />
            НАЗАД
          </button>
          <h1 className="text-4xl font-light text-foreground mb-2">
            CINEMA ONLINE
          </h1>
          <p className="text-lg text-muted-foreground">
            YOUR PLAN
          </p>
        </div>

        {/* Plans Grid */}
        <div className="grid md:grid-cols-2 gap-8 max-w-4xl mx-auto">
          {plans.map((plan) => (
            <div
              key={plan.id}
              className={`p-8 rounded-2xl border-2 transition-colors ${
                plan.featured
                  ? "border-accent bg-card"
                  : "border-border bg-card hover:border-accent"
              }`}
            >
              <h2 className="text-4xl font-light text-foreground mb-2">
                {plan.name}
              </h2>
              <div className="mb-6">
                <p className="text-4xl font-bold text-foreground">
                  {plan.price}
                  <span className="text-lg">₽</span>
                </p>
                <p className="text-sm text-muted-foreground mt-2">
                  {plan.duration}
                </p>
              </div>

              <ul className="space-y-3 mb-8">
                {plan.features.map((feature, index) => (
                  <li key={index} className="flex items-start gap-3">
                    <Check className="w-5 h-5 text-accent flex-shrink-0 mt-0.5" />
                    <span className="text-foreground">{feature}</span>
                  </li>
                ))}
              </ul>

              <button
                className={`w-full px-6 py-3 rounded-xl font-medium transition-colors ${
                  plan.featured
                    ? "bg-accent text-accent-foreground hover:bg-accent/90"
                    : "bg-primary text-primary-foreground hover:bg-primary/90"
                }`}
              >
                Оформить подписку
              </button>
            </div>
          ))}
        </div>

        {/* Benefits Section */}
        <section className="mt-16 p-8 bg-card rounded-2xl border border-border">
          <h2 className="text-2xl font-bold text-foreground mb-4">
            Получите доступ ко всем фильмам
          </h2>
          <p className="text-foreground mb-6">
            Оформите подписку и смотрите без ограничений!
          </p>
          <button className="px-8 py-3 bg-accent text-accent-foreground rounded-xl font-medium hover:bg-accent/90 transition-colors">
            Выбрать подписку
          </button>
        </section>
      </main>
    </div>
  );
}
