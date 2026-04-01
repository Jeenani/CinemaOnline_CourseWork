package ru.cinema.main.fragment;

import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.GridLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import ru.cinema.main.FilmDetailActivity;
import ru.cinema.main.R;
import ru.cinema.main.adapter.MovieGridAdapter;
import ru.cinema.main.api.ApiClient;
import ru.cinema.main.model.ApiResponse;
import ru.cinema.main.model.Movie;
import ru.cinema.main.model.PagedResponse;

public class FilmsFragment extends Fragment {

    private MovieGridAdapter adapter;
    private int currentPage = 1;
    private boolean isLoading = false;
    private boolean hasMore = true;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_films, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        RecyclerView rv = view.findViewById(R.id.rv_films);
        GridLayoutManager layoutManager = new GridLayoutManager(getContext(), 2);
        rv.setLayoutManager(layoutManager);
        adapter = new MovieGridAdapter(this::openMovie);
        rv.setAdapter(adapter);

        // Pagination
        rv.addOnScrollListener(new RecyclerView.OnScrollListener() {
            @Override
            public void onScrolled(@NonNull RecyclerView recyclerView, int dx, int dy) {
                super.onScrolled(recyclerView, dx, dy);
                int totalItems = layoutManager.getItemCount();
                int lastVisible = layoutManager.findLastVisibleItemPosition();
                if (!isLoading && hasMore && lastVisible >= totalItems - 4) {
                    currentPage++;
                    loadMovies();
                }
            }
        });

        loadMovies();
    }

    private void loadMovies() {
        isLoading = true;
        ApiClient.getService().getMovies(null, null, null, null, "date", true, currentPage, 20)
                .enqueue(new Callback<ApiResponse<PagedResponse<Movie>>>() {
                    @Override
                    public void onResponse(Call<ApiResponse<PagedResponse<Movie>>> call, Response<ApiResponse<PagedResponse<Movie>>> response) {
                        isLoading = false;
                        if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                            PagedResponse<Movie> paged = response.body().getData();
                            if (paged != null && paged.getItems() != null && isAdded()) {
                                if (currentPage == 1) {
                                    adapter.setMovies(paged.getItems());
                                } else {
                                    adapter.addMovies(paged.getItems());
                                }
                                hasMore = currentPage < paged.getTotalPages();
                            }
                        }
                    }

                    @Override
                    public void onFailure(Call<ApiResponse<PagedResponse<Movie>>> call, Throwable t) {
                        isLoading = false;
                    }
                });
    }

    private void openMovie(Movie movie) {
        Intent intent = new Intent(getContext(), FilmDetailActivity.class);
        intent.putExtra(FilmDetailActivity.EXTRA_MOVIE_ID, movie.getId());
        startActivity(intent);
    }
}

