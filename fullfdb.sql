dotnet ef dbcontext scaffold "Server=localhost;Port=5432;Database=CinemaOnline;Username=postgres;Password=r2d2m" Npgsql.EntityFrameworkCore.PostgreSQL -o Models --context CinemaOnlineContext

-- Тарифные планы
CREATE TABLE subscriptions (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    price NUMERIC(10, 2) NOT NULL,
    duration_days INTEGER NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT now()
);

-- Пользователи
CREATE TABLE users (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    name VARCHAR(100),
    role VARCHAR(20) DEFAULT 'user',
    subscription_id BIGINT,
    has_subscription BOOLEAN DEFAULT false,
    subscription_start_date TIMESTAMP,
    subscription_end_date TIMESTAMP,
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP DEFAULT now(),
    
    CONSTRAINT users_role_check CHECK (role IN ('user', 'admin')),
    CONSTRAINT users_subscription_fk FOREIGN KEY (subscription_id) 
        REFERENCES subscriptions(id) ON DELETE SET NULL
);

-- История подписок
CREATE TABLE subscription_history (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id BIGINT NOT NULL,
    subscription_id BIGINT NOT NULL,
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT now(),
    
    CONSTRAINT sub_history_user_fk FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT sub_history_subscription_fk FOREIGN KEY (subscription_id) 
        REFERENCES subscriptions(id) ON DELETE CASCADE
);

-- Платежи
CREATE TABLE payments (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id BIGINT NOT NULL,
    subscription_id BIGINT NOT NULL,
    amount NUMERIC(10, 2) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'pending',
    payment_method VARCHAR(50),
    transaction_id VARCHAR(255) UNIQUE,
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP DEFAULT now(),
    
    CONSTRAINT payments_status_check CHECK (status IN ('pending', 'paid', 'failed', 'refunded')),
    CONSTRAINT payments_user_fk FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT payments_subscription_fk FOREIGN KEY (subscription_id) 
        REFERENCES subscriptions(id) ON DELETE CASCADE
);

-- Жанры
CREATE TABLE genres (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    display_name VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT now()
);

-- Коллекции
CREATE TABLE collections (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    is_featured BOOLEAN DEFAULT false,
    display_order INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT now()
);

-- Фильмы
CREATE TABLE movies (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    release_year INTEGER,
    duration_minutes INTEGER,
    video_url TEXT NOT NULL,
    vk_video_url TEXT,
    poster_url TEXT,
    banner_url TEXT,
    country VARCHAR(100),
    director VARCHAR(255),
    need_subscription BOOLEAN DEFAULT false,
    is_published BOOLEAN DEFAULT true,
    average_rating NUMERIC(3,2) DEFAULT 0,
    ratings_count INTEGER DEFAULT 0,
    view_count BIGINT DEFAULT 0,
    comment_count INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP DEFAULT now(),
    published_at TIMESTAMP
);

-- Связь фильмы-жанры (M:N)
CREATE TABLE movie_genres (
    movie_id BIGINT NOT NULL,
    genre_id BIGINT NOT NULL,
    PRIMARY KEY (movie_id, genre_id),
    CONSTRAINT movie_genres_movie_fk FOREIGN KEY (movie_id) 
        REFERENCES movies(id) ON DELETE CASCADE,
    CONSTRAINT movie_genres_genre_fk FOREIGN KEY (genre_id) 
        REFERENCES genres(id) ON DELETE CASCADE
);

-- Связь коллекции-фильмы (M:N)
CREATE TABLE collection_movies (
    collection_id BIGINT NOT NULL,
    movie_id BIGINT NOT NULL,
    position INTEGER DEFAULT 0,
    PRIMARY KEY (collection_id, movie_id),
    CONSTRAINT collection_movies_collection_fk FOREIGN KEY (collection_id) 
        REFERENCES collections(id) ON DELETE CASCADE,
    CONSTRAINT collection_movies_movie_fk FOREIGN KEY (movie_id) 
        REFERENCES movies(id) ON DELETE CASCADE
);

-- Рейтинги
CREATE TABLE ratings (
    user_id BIGINT NOT NULL,
    movie_id BIGINT NOT NULL,
    rating INTEGER NOT NULL CHECK (rating BETWEEN 1 AND 5),
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP DEFAULT now(),
    PRIMARY KEY (user_id, movie_id),
    CONSTRAINT ratings_user_fk FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT ratings_movie_fk FOREIGN KEY (movie_id) 
        REFERENCES movies(id) ON DELETE CASCADE
);

-- Комментарии
CREATE TABLE comments (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id BIGINT NOT NULL,
    movie_id BIGINT NOT NULL,
    content TEXT NOT NULL,
    is_visible BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP DEFAULT now(),
    
    CONSTRAINT comments_user_fk FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT comments_movie_fk FOREIGN KEY (movie_id) 
        REFERENCES movies(id) ON DELETE CASCADE
);

-- Избранное
CREATE TABLE favorites (
    user_id BIGINT NOT NULL,
    movie_id BIGINT NOT NULL,
    created_at TIMESTAMP DEFAULT now(),
    PRIMARY KEY (user_id, movie_id),
    CONSTRAINT favorites_user_fk FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT favorites_movie_fk FOREIGN KEY (movie_id) 
        REFERENCES movies(id) ON DELETE CASCADE
);

-- История просмотров
CREATE TABLE view_history (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id BIGINT NOT NULL,
    movie_id BIGINT NOT NULL,
    progress_seconds INTEGER DEFAULT 0,
    completed BOOLEAN DEFAULT false,
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP DEFAULT now(),
    
    CONSTRAINT view_history_user_fk FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT view_history_movie_fk FOREIGN KEY (movie_id) 
        REFERENCES movies(id) ON DELETE CASCADE
);

-- ============================================
-- ИНДЕКСЫ
-- ============================================

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_subscription_end_date ON users(subscription_end_date);
CREATE INDEX idx_payments_user_id ON payments(user_id);
CREATE INDEX idx_payments_status ON payments(status);
CREATE INDEX idx_movies_release_year ON movies(release_year DESC);
CREATE INDEX idx_movies_created_at ON movies(created_at DESC);
CREATE INDEX idx_movies_average_rating ON movies(average_rating DESC);
CREATE INDEX idx_comments_movie_id ON comments(movie_id);
CREATE INDEX idx_ratings_movie_id ON ratings(movie_id);
CREATE INDEX idx_view_history_user_id ON view_history(user_id);
CREATE UNIQUE INDEX idx_view_history_user_movie ON view_history(user_id, movie_id);

-- Полнотекстовый поиск
CREATE INDEX idx_movies_title_search ON movies USING gin(to_tsvector('russian', title));

-- ============================================
-- ТРИГГЕРЫ
-- ============================================

-- 1. Автоматическое обновление updated_at
CREATE OR REPLACE FUNCTION update_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_users_updated_at 
    BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER trigger_movies_updated_at 
    BEFORE UPDATE ON movies
    FOR EACH ROW EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER trigger_comments_updated_at 
    BEFORE UPDATE ON comments
    FOR EACH ROW EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER trigger_ratings_updated_at 
    BEFORE UPDATE ON ratings
    FOR EACH ROW EXECUTE FUNCTION update_updated_at();

-- 2. Пересчет рейтинга фильма
CREATE OR REPLACE FUNCTION update_movie_rating()
RETURNS TRIGGER AS $$
DECLARE
    v_movie_id BIGINT;
BEGIN
    v_movie_id := COALESCE(NEW.movie_id, OLD.movie_id);
    
    UPDATE movies SET
        average_rating = COALESCE((
            SELECT ROUND(AVG(rating)::numeric, 2)
            FROM ratings WHERE movie_id = v_movie_id
        ), 0),
        ratings_count = (
            SELECT COUNT(*) FROM ratings WHERE movie_id = v_movie_id
        )
    WHERE id = v_movie_id;
    
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_rating_insert
    AFTER INSERT ON ratings
    FOR EACH ROW EXECUTE FUNCTION update_movie_rating();

CREATE TRIGGER trigger_rating_update
    AFTER UPDATE ON ratings
    FOR EACH ROW EXECUTE FUNCTION update_movie_rating();

CREATE TRIGGER trigger_rating_delete
    AFTER DELETE ON ratings
    FOR EACH ROW EXECUTE FUNCTION update_movie_rating();

-- 3. Подсчет комментариев
CREATE OR REPLACE FUNCTION update_comment_count()
RETURNS TRIGGER AS $$
DECLARE
    v_movie_id BIGINT;
BEGIN
    v_movie_id := COALESCE(NEW.movie_id, OLD.movie_id);
    
    UPDATE movies SET
        comment_count = (
            SELECT COUNT(*) 
            FROM comments 
            WHERE movie_id = v_movie_id AND is_visible = true
        )
    WHERE id = v_movie_id;
    
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_comment_insert
    AFTER INSERT ON comments
    FOR EACH ROW EXECUTE FUNCTION update_comment_count();

CREATE TRIGGER trigger_comment_update
    AFTER UPDATE ON comments
    FOR EACH ROW EXECUTE FUNCTION update_comment_count();

CREATE TRIGGER trigger_comment_delete
    AFTER DELETE ON comments
    FOR EACH ROW EXECUTE FUNCTION update_comment_count();

-- 4. Счетчик просмотров (только первый просмотр)
CREATE OR REPLACE FUNCTION increment_view_count()
RETURNS TRIGGER AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM view_history 
        WHERE user_id = NEW.user_id 
          AND movie_id = NEW.movie_id 
          AND id != NEW.id
    ) THEN
        UPDATE movies 
        SET view_count = view_count + 1 
        WHERE id = NEW.movie_id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_view_count
    AFTER INSERT ON view_history
    FOR EACH ROW EXECUTE FUNCTION increment_view_count();

-- 5. Автообновление статуса подписки
CREATE OR REPLACE FUNCTION update_subscription_status()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.subscription_end_date IS NOT NULL THEN
        NEW.has_subscription = (NEW.subscription_end_date > now());
    ELSE
        NEW.has_subscription = false;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_subscription_status
    BEFORE INSERT OR UPDATE OF subscription_end_date ON users
    FOR EACH ROW EXECUTE FUNCTION update_subscription_status();

-- 6. Установка published_at при публикации
CREATE OR REPLACE FUNCTION set_published_at()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.is_published = true AND (OLD IS NULL OR OLD.is_published = false) THEN
        NEW.published_at = now();
    ELSIF NEW.is_published = false THEN
        NEW.published_at = NULL;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_published_at
    BEFORE INSERT OR UPDATE OF is_published ON movies
    FOR EACH ROW EXECUTE FUNCTION set_published_at();

-- ============================================
-- ТЕСТОВЫЕ ДАННЫЕ
-- ============================================

-- Subscriptions
INSERT INTO subscriptions (name, price, duration_days, description) VALUES
('Basic', 5.99, 30, 'Все премиум фильмы в HD качестве'),
('Premium', 9.99, 30, 'Все премиум фильмы в HD без рекламы с поддержкой 24/7');

-- Genres
INSERT INTO genres (name, display_name) VALUES
('fantasy', 'Фантастика'),
('action', 'Боевик'),
('adventure', 'Приключения'),
('comedy', 'Комедия'),
('family', 'Семейные'),
('superhero', 'Супергерои'),
('thriller', 'Триллер'),
('drama', 'Драма');

-- Collections
INSERT INTO collections (name, description, is_featured, display_order) VALUES
('Подборка месяца', 'Лучшие фильмы этого месяца', true, 1),
('Новые фильмы', 'Недавно добавленные фильмы', true, 2);

-- Users (пароль: password123)
INSERT INTO users (email, password_hash, name, role) VALUES
('admin@cinema.com', '$2a$11$vV8YgZw3QZPxQXJz3X7JNuH8yJ3LqKZv3Y8dXqXYZ8qXqXqXqXqXq', 'Admin', 'admin'),
('user@test.com', '$2a$11$vV8YgZw3QZPxQXJz3X7JNuH8yJ3LqKZv3Y8dXqXYZ8qXqXqXqXqXq', 'Test User', 'user');

-- Movies
INSERT INTO movies (title, description, release_year, duration_minutes, video_url, poster_url, banner_url, country, director, need_subscription, is_published) VALUES
('Гарри Поттер и Дары Смерти', 'Гарри Поттера ждёт самое страшное испытание в жизни - смертельная схватка с Волан-де-Мортом.', 2010, 146, 'https://example.com/video/hp7.mp4', 'https://upload.wikimedia.org/wikipedia/ru/f/f4/Harry_Potter_and_the_Deathly_Hallows._Part_2_%E2%80%94_movie.jpg', 'https://images.iptv.rt.ru/images/c6tmrk3ir4sslltredr0.jpg' 'США', 'Дэвид Йейтс', true, true),
('Мстители: Финал', 'После разрушительных событий герои пытаются восстановить порядок во вселенной.', 2019, 181, 'https://example.com/video/avengers.mp4', 'https://upload.wikimedia.org/wikipedia/ru/0/0d/Avengers_Endgame_poster.jpg', 'https://images.iptv.rt.ru/images/c6u8fajir4sslltu2p0g.jpg', 'США', 'Энтони и Джо Руссо', true, true);

-- Movie Genres
INSERT INTO movie_genres (movie_id, genre_id) VALUES
(1, 1), (1, 3), (1, 5),  -- Harry Potter: fantasy, adventure, family
(2, 6), (2, 2), (2, 1);  -- Avengers: superhero, action, fantasy

-- Collection Movies
INSERT INTO collection_movies (collection_id, movie_id, position) VALUES
(1, 1, 1), (1, 2, 2),
(2, 2, 1);

-- Ratings
INSERT INTO ratings (user_id, movie_id, rating) VALUES
(2, 1, 5),
(2, 2, 4);

-- Comments
INSERT INTO comments (user_id, movie_id, content) VALUES
(2, 1, 'Отличный фильм! Достойное завершение саги.'),
(2, 2, 'Эпическая битва супергероев!');

-- Favorites
INSERT INTO favorites (user_id, movie_id) VALUES
(2, 1),
(2, 2);

-- View History
INSERT INTO view_history (user_id, movie_id, progress_seconds, completed) VALUES
(2, 1, 8760, true),
(2, 2, 5000, false);

-- Payments
INSERT INTO payments (user_id, subscription_id, amount, status, payment_method, transaction_id) VALUES
(2, 1, 5.99, 'paid', 'card', 'txn_123456789'),
(2, 2, 9.99, 'pending', 'card', 'txn_987654321');

-- Subscription History
INSERT INTO subscription_history (user_id, subscription_id, start_date, end_date, is_active) VALUES
(2, 1, now() - interval '30 days', now(), false),
(2, 2, now(), now() + interval '30 days', true);

-- ============================================
-- ПОЛЕЗНЫЕ ПРЕДСТАВЛЕНИЯ (VIEWS)
-- ============================================

-- Фильмы с жанрами
CREATE OR REPLACE VIEW v_movies_with_genres AS
SELECT 
    m.id,
    m.title,
    m.description,
    m.release_year,
    m.duration_minutes,
    m.poster_url,
    m.average_rating,
    m.ratings_count,
    m.view_count,
    m.need_subscription,
    ARRAY_AGG(g.display_name) as genres
FROM movies m
LEFT JOIN movie_genres mg ON m.id = mg.movie_id
LEFT JOIN genres g ON mg.genre_id = g.id
WHERE m.is_published = true
GROUP BY m.id;

-- Активные пользователи с подписками
CREATE OR REPLACE VIEW v_active_subscriptions AS
SELECT 
    u.id as user_id,
    u.email,
    u.name,
    s.name as subscription_name,
    u.subscription_start_date,
    u.subscription_end_date,
    u.has_subscription
FROM users u
INNER JOIN subscriptions s ON u.subscription_id = s.id
WHERE u.has_subscription = true;

-- Завершение
SELECT 'Database schema created successfully!' as message;