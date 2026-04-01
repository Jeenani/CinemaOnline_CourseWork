package ru.cinema.main;

import android.content.Intent;
import android.content.pm.ActivityInfo;
import android.os.Bundle;
import android.view.View;
import android.view.WindowManager;
import android.webkit.WebChromeClient;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.EditText;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;

import androidx.activity.OnBackPressedCallback;
import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import ru.cinema.main.adapter.CommentAdapter;
import ru.cinema.main.adapter.MovieHorizontalAdapter;
import ru.cinema.main.api.ApiClient;
import ru.cinema.main.model.ApiResponse;
import ru.cinema.main.model.Comment;
import ru.cinema.main.model.Movie;
import ru.cinema.main.model.User;
import ru.cinema.main.util.SessionManager;

public class FilmDetailActivity extends AppCompatActivity {

    public static final String EXTRA_MOVIE_ID = "movie_id";

    private SessionManager sessionManager;
    private long movieId;
    private Movie currentMovie;
    private int selectedRating = 0;

    private ImageView ivPoster;
    private TextView tvTitle, tvSubtitle, tvGenre, tvYear, tvDirector, tvDuration, tvDescription;
    private LinearLayout llRating, llActions, llCommentInput, llCommentStars;
    private FrameLayout llPlayer, flFullscreen;
    private WebView wvPlayer;
    private View customView;
    private WebChromeClient.CustomViewCallback customViewCallback;
    private WebChromeClient webChromeClient;
    private TextView btnWatch, btnFavorite, btnShare, btnLoginToWatch, btnSendComment, tvCommentAvatar, tvCommentsTitle;
    private TextView btnSubscribeToWatch;
    private LinearLayout llSubscriptionRequired;
    private EditText etComment;
    private RecyclerView rvRelated, rvComments;
    private CommentAdapter commentAdapter;
    private MovieHorizontalAdapter relatedAdapter;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_film_detail);

        sessionManager = new SessionManager(this);
        movieId = getIntent().getLongExtra(EXTRA_MOVIE_ID, -1);

        initViews();
        loadMovie();

        // Handle back press for fullscreen video
        getOnBackPressedDispatcher().addCallback(this, new OnBackPressedCallback(true) {
            @Override
            public void handleOnBackPressed() {
                if (customView != null && webChromeClient != null) {
                    webChromeClient.onHideCustomView();
                } else {
                    setEnabled(false);
                    getOnBackPressedDispatcher().onBackPressed();
                }
            }
        });
    }

    private void initViews() {
        findViewById(R.id.btn_back).setOnClickListener(v -> finish());

        ivPoster = findViewById(R.id.iv_poster);
        tvTitle = findViewById(R.id.tv_title);
        tvSubtitle = findViewById(R.id.tv_subtitle);
        tvGenre = findViewById(R.id.tv_genre);
        tvYear = findViewById(R.id.tv_year);
        tvDirector = findViewById(R.id.tv_director);
        tvDuration = findViewById(R.id.tv_duration);
        tvDescription = findViewById(R.id.tv_description);
        llRating = findViewById(R.id.ll_rating);
        llActions = findViewById(R.id.ll_actions);
        llPlayer = findViewById(R.id.ll_player);
        flFullscreen = findViewById(R.id.fl_fullscreen);
        wvPlayer = findViewById(R.id.wv_player);
        llCommentInput = findViewById(R.id.ll_comment_input);
        llCommentStars = findViewById(R.id.ll_comment_stars);
        btnWatch = findViewById(R.id.btn_watch);
        btnFavorite = findViewById(R.id.btn_favorite);
        btnShare = findViewById(R.id.btn_share);
        btnLoginToWatch = findViewById(R.id.btn_login_to_watch);
        btnSendComment = findViewById(R.id.btn_send_comment);
        tvCommentAvatar = findViewById(R.id.tv_comment_avatar);
        tvCommentsTitle = findViewById(R.id.tv_comments_title);
        etComment = findViewById(R.id.et_comment);

        // Setup WebView for video player (iframe support)
        setupWebView();

        rvRelated = findViewById(R.id.rv_related);
        rvRelated.setLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.HORIZONTAL, false));
        relatedAdapter = new MovieHorizontalAdapter(movie -> {
            Intent intent = new Intent(this, FilmDetailActivity.class);
            intent.putExtra(EXTRA_MOVIE_ID, movie.getId());
            startActivity(intent);
        });
        rvRelated.setAdapter(relatedAdapter);

        rvComments = findViewById(R.id.rv_comments);
        rvComments.setLayoutManager(new LinearLayoutManager(this));
        commentAdapter = new CommentAdapter();
        rvComments.setAdapter(commentAdapter);

        // Comment rating stars
        setupCommentStars();

        btnSendComment.setOnClickListener(v -> sendComment());
        btnLoginToWatch.setOnClickListener(v -> startActivity(new Intent(this, LoginActivity.class)));

        llSubscriptionRequired = findViewById(R.id.ll_subscription_required);
        btnSubscribeToWatch = findViewById(R.id.btn_subscribe_to_watch);
        btnSubscribeToWatch.setOnClickListener(v -> startActivity(new Intent(this, SubscriptionActivity.class)));

        btnWatch.setOnClickListener(v -> {
            if (currentMovie != null && currentMovie.isNeedSubscription()) {
                User user = sessionManager.getUser();
                if (user == null || !user.isHasSubscription()) {
                    Toast.makeText(this, getString(R.string.subscription_required), Toast.LENGTH_LONG).show();
                    startActivity(new Intent(this, SubscriptionActivity.class));
                    return;
                }
            }
            if (currentMovie != null && currentMovie.getVkVideoUrl() != null
                    && !currentMovie.getVkVideoUrl().isEmpty()) {
                loadVideoPlayer(currentMovie.getVkVideoUrl());
            }
        });

        btnFavorite.setOnClickListener(v -> toggleFavorite());
        btnShare.setOnClickListener(v -> shareMovie());
    }

    private void setupCommentStars() {
        llCommentStars.removeAllViews();
        for (int i = 1; i <= 5; i++) {
            final int starIndex = i;
            ImageView star = new ImageView(this);
            int size = (int) (20 * getResources().getDisplayMetrics().density);
            LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(size, size);
            params.setMarginEnd((int) (4 * getResources().getDisplayMetrics().density));
            star.setLayoutParams(params);
            star.setImageResource(R.drawable.ic_star_empty);
            star.setColorFilter(getColor(R.color.star_empty));
            star.setOnClickListener(v -> {
                selectedRating = starIndex;
                updateCommentStars();
            });
            llCommentStars.addView(star);
        }
    }

    private void updateCommentStars() {
        for (int i = 0; i < llCommentStars.getChildCount(); i++) {
            ImageView star = (ImageView) llCommentStars.getChildAt(i);
            if (i < selectedRating) {
                star.setImageResource(R.drawable.ic_star_filled);
                star.setColorFilter(getColor(R.color.star_filled));
            } else {
                star.setImageResource(R.drawable.ic_star_empty);
                star.setColorFilter(getColor(R.color.star_empty));
            }
        }
    }

    @SuppressWarnings("SetJavaScriptEnabled")
    private void setupWebView() {
        WebSettings settings = wvPlayer.getSettings();
        settings.setJavaScriptEnabled(true);
        settings.setDomStorageEnabled(true);
        settings.setMediaPlaybackRequiresUserGesture(false);
        settings.setAllowFileAccess(true);
        settings.setLoadWithOverviewMode(true);
        settings.setUseWideViewPort(true);

        wvPlayer.setWebViewClient(new WebViewClient());
        webChromeClient = new WebChromeClient() {
            @Override
            public void onShowCustomView(View view, CustomViewCallback callback) {
                // Fullscreen mode (like YouTube fullscreen button)
                customView = view;
                customViewCallback = callback;
                flFullscreen.addView(view);
                flFullscreen.setVisibility(View.VISIBLE);
                findViewById(R.id.scroll_view).setVisibility(View.GONE);
                getWindow().addFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN);
                setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_LANDSCAPE);
            }

            @Override
            public void onHideCustomView() {
                // Exit fullscreen
                if (customView != null) {
                    flFullscreen.removeView(customView);
                    customView = null;
                }
                if (customViewCallback != null) {
                    customViewCallback.onCustomViewHidden();
                    customViewCallback = null;
                }
                flFullscreen.setVisibility(View.GONE);
                findViewById(R.id.scroll_view).setVisibility(View.VISIBLE);
                getWindow().clearFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN);
                setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_UNSPECIFIED);
            }
        };
        wvPlayer.setWebChromeClient(webChromeClient);
    }

    private void loadVideoPlayer(String videoUrl) {
        llPlayer.setVisibility(View.VISIBLE);

        String html;
        String trimmed = videoUrl.trim();

        if (trimmed.contains("<iframe")) {
            // Already an iframe tag — wrap it in HTML with responsive sizing
            html = "<!DOCTYPE html><html><head>"
                    + "<meta name='viewport' content='width=device-width, initial-scale=1.0'>"
                    + "<style>body{margin:0;padding:0;background:#000;}"
                    + "iframe{width:100%;height:100%;position:absolute;top:0;left:0;border:0;}"
                    + ".container{position:relative;width:100%;padding-bottom:56.25%;height:0;overflow:hidden;}</style>"
                    + "</head><body><div class='container'>"
                    + trimmed
                    + "</div></body></html>";
        } else {
            // Plain URL — embed it as an iframe src (works for Rutube, VK Video, etc.)
            html = "<!DOCTYPE html><html><head>"
                    + "<meta name='viewport' content='width=device-width, initial-scale=1.0'>"
                    + "<style>body{margin:0;padding:0;background:#000;}"
                    + "iframe{width:100%;height:100%;position:absolute;top:0;left:0;border:0;}"
                    + ".container{position:relative;width:100%;padding-bottom:56.25%;height:0;overflow:hidden;}</style>"
                    + "</head><body><div class='container'>"
                    + "<iframe src='" + trimmed + "' allowfullscreen allow='autoplay; encrypted-media; fullscreen'></iframe>"
                    + "</div></body></html>";
        }

        wvPlayer.loadDataWithBaseURL(null, html, "text/html", "UTF-8", null);
    }

    private void loadMovie() {
        if (movieId == -1) return;

        Callback<ApiResponse<Movie>> callback = new Callback<ApiResponse<Movie>>() {
            @Override
            public void onResponse(Call<ApiResponse<Movie>> call, Response<ApiResponse<Movie>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    currentMovie = response.body().getData();
                    displayMovie();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<Movie>> call, Throwable t) {
                Toast.makeText(FilmDetailActivity.this, "Ошибка загрузки", Toast.LENGTH_SHORT).show();
            }
        };

        String token = sessionManager.getToken();
        if (token != null) {
            ApiClient.getService().getMovieAuth(movieId, token).enqueue(callback);
        } else {
            ApiClient.getService().getMovie(movieId).enqueue(callback);
        }
    }

    private void displayMovie() {
        if (currentMovie == null) return;

        tvTitle.setText(currentMovie.getTitle());
        tvSubtitle.setText(currentMovie.getReleaseYear() != null ?
                "(" + currentMovie.getReleaseYear() + ")" : "");

        if (currentMovie.getGenres() != null) {
            tvGenre.setText(String.join(", ", currentMovie.getGenres()));
        }
        tvYear.setText(currentMovie.getReleaseYear() != null ?
                String.valueOf(currentMovie.getReleaseYear()) : "—");
        tvDirector.setText(currentMovie.getDirector() != null ? currentMovie.getDirector() : "—");
        tvDuration.setText(currentMovie.getDurationMinutes() != null ?
                currentMovie.getDurationMinutes() + " мин" : "—");
        tvDescription.setText(currentMovie.getDescription() != null ? currentMovie.getDescription() : "");

        // Poster — prefer banner for wide preview, fallback to poster
        String imageUrl = currentMovie.getBannerUrl();
        if (imageUrl == null || imageUrl.isEmpty()) {
            imageUrl = currentMovie.getPosterUrl();
        }
        if (imageUrl != null && !imageUrl.isEmpty()) {
            String fullUrl = imageUrl.startsWith("http") ? imageUrl : ApiClient.getBaseUrl() + imageUrl;
            Glide.with(this).load(fullUrl).centerCrop().into(ivPoster);
        }

        // Rating stars
        llRating.removeAllViews();
        int rating = (int) Math.round(currentMovie.getAverageRating());
        for (int i = 0; i < 5; i++) {
            ImageView star = new ImageView(this);
            int size = (int) (22 * getResources().getDisplayMetrics().density);
            star.setLayoutParams(new LinearLayout.LayoutParams(size, size));
            if (i < rating) {
                star.setImageResource(R.drawable.ic_star_filled);
                star.setColorFilter(getColor(R.color.star_filled));
            } else {
                star.setImageResource(R.drawable.ic_star_empty);
                star.setColorFilter(getColor(R.color.star_empty));
            }
            llRating.addView(star);
        }

        // Auth-dependent UI
        boolean isAuth = sessionManager.isLoggedIn();
        User currentUser = isAuth ? sessionManager.getUser() : null;
        boolean hasSubscription = currentUser != null && currentUser.isHasSubscription();
        boolean needsSub = currentMovie.isNeedSubscription();

        llActions.setVisibility(isAuth ? View.VISIBLE : View.GONE);
        btnLoginToWatch.setVisibility(isAuth ? View.GONE : View.VISIBLE);
        llCommentInput.setVisibility(isAuth ? View.VISIBLE : View.GONE);
        tvCommentsTitle.setText(isAuth ? getString(R.string.leave_comment) : getString(R.string.comments_title));

        // Subscription-required block: show when user is logged in, movie needs subscription, but user has none
        if (isAuth && needsSub && !hasSubscription) {
            llSubscriptionRequired.setVisibility(View.VISIBLE);
            llPlayer.setVisibility(View.GONE);
            btnWatch.setVisibility(View.GONE);
        } else {
            llSubscriptionRequired.setVisibility(View.GONE);
            btnWatch.setVisibility(View.VISIBLE);

            // Show video player if auth and video URL exists and subscription is OK
            if (isAuth && currentMovie.getVkVideoUrl() != null
                    && !currentMovie.getVkVideoUrl().isEmpty()) {
                loadVideoPlayer(currentMovie.getVkVideoUrl());
            } else {
                llPlayer.setVisibility(View.GONE);
            }
        }

        if (isAuth) {
            String userName = sessionManager.getUser().getName();
            tvCommentAvatar.setText(userName != null && !userName.isEmpty() ?
                    String.valueOf(userName.charAt(0)).toUpperCase() : "?");

            // Update favorite button state
            if (currentMovie.getIsFavorite() != null && currentMovie.getIsFavorite()) {
                btnFavorite.setText("★ Избранное");
            }
        }

        // Load comments
        if (currentMovie.getComments() != null && !currentMovie.getComments().isEmpty()) {
            commentAdapter.setComments(currentMovie.getComments());
        } else {
            loadComments();
        }
    }

    private void loadComments() {
        ApiClient.getService().getComments(movieId).enqueue(new Callback<ApiResponse<List<Comment>>>() {
            @Override
            public void onResponse(Call<ApiResponse<List<Comment>>> call, Response<ApiResponse<List<Comment>>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    commentAdapter.setComments(response.body().getData());
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<Comment>>> call, Throwable t) {}
        });
    }

    private void sendComment() {
        String text = etComment.getText().toString().trim();
        if (text.isEmpty()) return;

        String token = sessionManager.getToken();
        if (token == null) return;

        Map<String, String> body = new HashMap<>();
        body.put("content", text);

        ApiClient.getService().addComment(movieId, token, body).enqueue(new Callback<ApiResponse<Long>>() {
            @Override
            public void onResponse(Call<ApiResponse<Long>> call, Response<ApiResponse<Long>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    etComment.setText("");
                    // Add comment locally
                    Comment c = new Comment();
                    c.setUserName(sessionManager.getUser().getName());
                    c.setContent(text);
                    commentAdapter.addComment(c);

                    // Also rate if selected
                    if (selectedRating > 0) {
                        rateMovie();
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<Long>> call, Throwable t) {
                Toast.makeText(FilmDetailActivity.this, "Ошибка отправки", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void rateMovie() {
        String token = sessionManager.getToken();
        if (token == null || selectedRating == 0) return;

        Map<String, Integer> body = new HashMap<>();
        body.put("rating", selectedRating);

        ApiClient.getService().rateMovie(movieId, token, body).enqueue(new Callback<ApiResponse<Boolean>>() {
            @Override
            public void onResponse(Call<ApiResponse<Boolean>> call, Response<ApiResponse<Boolean>> response) {
                if (response.isSuccessful()) {
                    Toast.makeText(FilmDetailActivity.this, "Оценка сохранена", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<Boolean>> call, Throwable t) {}
        });
    }

    private void toggleFavorite() {
        String token = sessionManager.getToken();
        if (token == null) return;

        boolean isFav = currentMovie.getIsFavorite() != null && currentMovie.getIsFavorite();

        Callback<ApiResponse<Boolean>> cb = new Callback<ApiResponse<Boolean>>() {
            @Override
            public void onResponse(Call<ApiResponse<Boolean>> call, Response<ApiResponse<Boolean>> response) {
                if (response.isSuccessful()) {
                    boolean newState = !isFav;
                    currentMovie.setIsFavorite(newState);
                    btnFavorite.setText(newState ? "★ Избранное" : getString(R.string.favorite_button));
                    Toast.makeText(FilmDetailActivity.this,
                            newState ? "Добавлено в избранное" : "Убрано из избранного",
                            Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<Boolean>> call, Throwable t) {}
        };

        if (isFav) {
            ApiClient.getService().removeFavorite(movieId, token).enqueue(cb);
        } else {
            ApiClient.getService().addFavorite(movieId, token).enqueue(cb);
        }
    }

    private void shareMovie() {
        if (currentMovie == null) return;
        Intent shareIntent = new Intent(Intent.ACTION_SEND);
        shareIntent.setType("text/plain");
        shareIntent.putExtra(Intent.EXTRA_TEXT, currentMovie.getTitle() + " — смотрите на Cinema Online!");
        startActivity(Intent.createChooser(shareIntent, getString(R.string.share_button)));
    }
}

