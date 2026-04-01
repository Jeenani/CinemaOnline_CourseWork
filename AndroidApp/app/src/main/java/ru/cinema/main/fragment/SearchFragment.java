package ru.cinema.main.fragment;

import android.content.Intent;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.GridLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import ru.cinema.main.FilmDetailActivity;
import ru.cinema.main.R;
import ru.cinema.main.adapter.MovieGridAdapter;
import ru.cinema.main.api.ApiClient;
import ru.cinema.main.model.ApiResponse;
import ru.cinema.main.model.Genre;
import ru.cinema.main.model.Movie;
import ru.cinema.main.model.PagedResponse;

public class SearchFragment extends Fragment {

    private EditText etSearch, etYearFrom, etYearTo;
    private Spinner spinnerGenre, spinnerSort;
    private RecyclerView rvResults;
    private TextView tvNoResults;
    private MovieGridAdapter adapter;

    private List<Genre> genresList = new ArrayList<>();
    private Long selectedGenreId = null;
    private String currentSort = "date";
    private boolean sortDescending = true;

    private Runnable searchRunnable;
    private final android.os.Handler handler = new android.os.Handler();

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_search, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        etSearch = view.findViewById(R.id.et_search);
        etYearFrom = view.findViewById(R.id.et_year_from);
        etYearTo = view.findViewById(R.id.et_year_to);
        spinnerGenre = view.findViewById(R.id.spinner_genre);
        spinnerSort = view.findViewById(R.id.spinner_sort);
        rvResults = view.findViewById(R.id.rv_search_results);
        tvNoResults = view.findViewById(R.id.tv_no_results);

        rvResults.setLayoutManager(new GridLayoutManager(getContext(), 2));
        adapter = new MovieGridAdapter(this::openMovie);
        rvResults.setAdapter(adapter);

        setupGenreSpinner();
        setupSortSpinner();
        setupSearchListeners();

        doSearch();
    }

    private void setupGenreSpinner() {
        List<String> genreNames = new ArrayList<>();
        genreNames.add(getString(R.string.all_genres));

        ArrayAdapter<String> genreAdapter = new ArrayAdapter<>(
                requireContext(), R.layout.item_spinner, genreNames);
        genreAdapter.setDropDownViewResource(R.layout.item_spinner);
        spinnerGenre.setAdapter(genreAdapter);

        // Load genres from API
        ApiClient.getService().getGenres().enqueue(new Callback<ApiResponse<List<Genre>>>() {
            @Override
            public void onResponse(Call<ApiResponse<List<Genre>>> call, Response<ApiResponse<List<Genre>>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess() && isAdded()) {
                    List<Genre> genres = response.body().getData();
                    if (genres != null) {
                        genresList.clear();
                        genresList.addAll(genres);
                        genreNames.clear();
                        genreNames.add(getString(R.string.all_genres));
                        for (Genre g : genres) {
                            genreNames.add(g.getDisplayName());
                        }
                        genreAdapter.notifyDataSetChanged();
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<Genre>>> call, Throwable t) {}
        });

        spinnerGenre.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                if (position == 0) {
                    selectedGenreId = null;
                } else if (position - 1 < genresList.size()) {
                    selectedGenreId = genresList.get(position - 1).getId();
                }
                doSearch();
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {}
        });
    }

    private void setupSortSpinner() {
        String[] sortOptions = {
                getString(R.string.sort_newest),
                getString(R.string.sort_oldest),
                getString(R.string.sort_rating)
        };

        ArrayAdapter<String> sortAdapter = new ArrayAdapter<>(
                requireContext(), R.layout.item_spinner, sortOptions);
        sortAdapter.setDropDownViewResource(R.layout.item_spinner);
        spinnerSort.setAdapter(sortAdapter);

        spinnerSort.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                switch (position) {
                    case 0:
                        currentSort = "date";
                        sortDescending = true;
                        break;
                    case 1:
                        currentSort = "date";
                        sortDescending = false;
                        break;
                    case 2:
                        currentSort = "rating";
                        sortDescending = true;
                        break;
                }
                doSearch();
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {}
        });
    }

    private void setupSearchListeners() {
        TextWatcher watcher = new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {}

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {}

            @Override
            public void afterTextChanged(Editable s) {
                handler.removeCallbacks(searchRunnable);
                searchRunnable = () -> doSearch();
                handler.postDelayed(searchRunnable, 500);
            }
        };

        etSearch.addTextChangedListener(watcher);
        etYearFrom.addTextChangedListener(watcher);
        etYearTo.addTextChangedListener(watcher);
    }

    private void doSearch() {
        String search = etSearch.getText().toString().trim();
        if (search.isEmpty()) search = null;

        Integer yearFrom = null;
        Integer yearTo = null;
        try {
            String yf = etYearFrom.getText().toString().trim();
            if (!yf.isEmpty()) yearFrom = Integer.parseInt(yf);
        } catch (NumberFormatException ignored) {}
        try {
            String yt = etYearTo.getText().toString().trim();
            if (!yt.isEmpty()) yearTo = Integer.parseInt(yt);
        } catch (NumberFormatException ignored) {}

        ApiClient.getService().getMovies(search, selectedGenreId, yearFrom, yearTo, currentSort, sortDescending, 1, 50)
                .enqueue(new Callback<ApiResponse<PagedResponse<Movie>>>() {
                    @Override
                    public void onResponse(Call<ApiResponse<PagedResponse<Movie>>> call, Response<ApiResponse<PagedResponse<Movie>>> response) {
                        if (response.isSuccessful() && response.body() != null && response.body().isSuccess() && isAdded()) {
                            PagedResponse<Movie> paged = response.body().getData();
                            if (paged != null && paged.getItems() != null) {
                                adapter.setMovies(paged.getItems());
                                tvNoResults.setVisibility(paged.getItems().isEmpty() ? View.VISIBLE : View.GONE);
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

