package ru.cinema.main.fragment;

import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.GridLayoutManager;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;

import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import ru.cinema.main.FilmDetailActivity;
import ru.cinema.main.LoginActivity;
import ru.cinema.main.R;
import ru.cinema.main.adapter.MovieGridAdapter;
import ru.cinema.main.adapter.MovieHorizontalAdapter;
import ru.cinema.main.api.ApiClient;
import ru.cinema.main.model.ApiResponse;
import ru.cinema.main.model.CollectionResponse;
import ru.cinema.main.model.Movie;
import ru.cinema.main.model.PagedResponse;
import ru.cinema.main.util.SessionManager;

public class HomeFragment extends Fragment {

    private SessionManager sessionManager;
    private MovieHorizontalAdapter monthlyAdapter;
    private MovieGridAdapter newMoviesAdapter;
    private ImageView ivFeatured;
    private TextView tvFeaturedTitle, tvFeaturedSubtitle, btnFeaturedWatch, btnFeaturedFavorite;
    private Movie featuredMovie;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_home, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        sessionManager = new SessionManager(requireContext());

        ivFeatured = view.findViewById(R.id.iv_featured);
        tvFeaturedTitle = view.findViewById(R.id.tv_featured_title);
        tvFeaturedSubtitle = view.findViewById(R.id.tv_featured_subtitle);
        btnFeaturedWatch = view.findViewById(R.id.btn_featured_watch);
        btnFeaturedFavorite = view.findViewById(R.id.btn_featured_favorite);

        // Monthly picks - horizontal
        RecyclerView rvMonthly = view.findViewById(R.id.rv_monthly_picks);
        rvMonthly.setLayoutManager(new LinearLayoutManager(getContext(), LinearLayoutManager.HORIZONTAL, false));
        monthlyAdapter = new MovieHorizontalAdapter(this::openMovie);
        rvMonthly.setAdapter(monthlyAdapter);

        // New movies - grid
        RecyclerView rvNew = view.findViewById(R.id.rv_new_movies);
        rvNew.setLayoutManager(new GridLayoutManager(getContext(), 2));
        newMoviesAdapter = new MovieGridAdapter(this::openMovie);
        rvNew.setAdapter(newMoviesAdapter);

        // Button clicks
        btnFeaturedWatch.setOnClickListener(v -> {
            if (sessionManager.isLoggedIn() && featuredMovie != null) {
                openMovie(featuredMovie);
            } else {
                startActivity(new Intent(getContext(), LoginActivity.class));
            }
        });

        btnFeaturedFavorite.setOnClickListener(v -> {
            if (!sessionManager.isLoggedIn()) {
                startActivity(new Intent(getContext(), LoginActivity.class));
            }
        });

        // Update button texts based on auth
        if (!sessionManager.isLoggedIn()) {
            btnFeaturedWatch.setText(R.string.login_to_watch);
            btnFeaturedFavorite.setVisibility(View.GONE);
        }

        loadFeaturedMovie();
        loadCollections();
        loadNewMovies();
    }

    private void loadFeaturedMovie() {
        ApiClient.getService().getRandomMovie().enqueue(new Callback<ApiResponse<Movie>>() {
            @Override
            public void onResponse(Call<ApiResponse<Movie>> call, Response<ApiResponse<Movie>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    featuredMovie = response.body().getData();
                    if (featuredMovie != null && isAdded()) {
                        tvFeaturedTitle.setText(featuredMovie.getTitle());
                        String subtitle = featuredMovie.getReleaseYear() != null ?
                                featuredMovie.getTitle() + " (" + featuredMovie.getReleaseYear() + ")" : "";
                        tvFeaturedSubtitle.setText(subtitle);

                        String bannerUrl = featuredMovie.getBannerUrl() != null ?
                                featuredMovie.getBannerUrl() : featuredMovie.getPosterUrl();
                        if (bannerUrl != null && !bannerUrl.isEmpty()) {
                            String fullUrl = bannerUrl.startsWith("http") ? bannerUrl : ApiClient.getBaseUrl() + bannerUrl;
                            Glide.with(requireContext()).load(fullUrl).centerCrop().into(ivFeatured);
                            ivFeatured.setAlpha(0.3f);
                        }
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<Movie>> call, Throwable t) {}
        });
    }

    private void loadCollections() {
        ApiClient.getService().getFeaturedCollections().enqueue(new Callback<ApiResponse<List<CollectionResponse>>>() {
            @Override
            public void onResponse(Call<ApiResponse<List<CollectionResponse>>> call, Response<ApiResponse<List<CollectionResponse>>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    List<CollectionResponse> collections = response.body().getData();
                    if (collections != null && !collections.isEmpty() && isAdded()) {
                        // Use first collection as monthly picks
                        CollectionResponse first = collections.get(0);
                        if (first.getMovies() != null) {
                            monthlyAdapter.setMovies(first.getMovies());
                        }
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<CollectionResponse>>> call, Throwable t) {}
        });
    }

    private void loadNewMovies() {
        ApiClient.getService().getMovies(null, null, null, null, "date", true, 1, 6)
                .enqueue(new Callback<ApiResponse<PagedResponse<Movie>>>() {
                    @Override
                    public void onResponse(Call<ApiResponse<PagedResponse<Movie>>> call, Response<ApiResponse<PagedResponse<Movie>>> response) {
                        if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                            PagedResponse<Movie> paged = response.body().getData();
                            if (paged != null && paged.getItems() != null && isAdded()) {
                                newMoviesAdapter.setMovies(paged.getItems());
                            }
                        }
                    }

                    @Override
                    public void onFailure(Call<ApiResponse<PagedResponse<Movie>>> call, Throwable t) {}
                });
    }

    private void openMovie(Movie movie) {
        Intent intent = new Intent(getContext(), FilmDetailActivity.class);
        intent.putExtra(FilmDetailActivity.EXTRA_MOVIE_ID, movie.getId());
        startActivity(intent);
    }
}

