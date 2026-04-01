package ru.cinema.main.fragment;

import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.GridLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import ru.cinema.main.FilmDetailActivity;
import ru.cinema.main.LoginActivity;
import ru.cinema.main.R;
import ru.cinema.main.SubscriptionActivity;
import ru.cinema.main.adapter.MovieGridAdapter;
import ru.cinema.main.api.ApiClient;
import ru.cinema.main.model.ApiResponse;
import ru.cinema.main.model.Movie;
import ru.cinema.main.model.SubscriptionInfo;
import ru.cinema.main.model.User;
import ru.cinema.main.util.SessionManager;

public class ProfileFragment extends Fragment {

    private SessionManager sessionManager;
    private TextView tvAvatarLetter, tvUserName, tvUserEmail, tvSubTitle, tvSubHint, btnUpgrade;
    private RecyclerView rvFavorites;
    private MovieGridAdapter favoritesAdapter;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_profile, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        sessionManager = new SessionManager(requireContext());

        if (!sessionManager.isLoggedIn()) {
            startActivity(new Intent(getContext(), LoginActivity.class));
            return;
        }

        tvAvatarLetter = view.findViewById(R.id.tv_avatar_letter);
        tvUserName = view.findViewById(R.id.tv_user_name);
        tvUserEmail = view.findViewById(R.id.tv_user_email);
        tvSubTitle = view.findViewById(R.id.tv_sub_title);
        tvSubHint = view.findViewById(R.id.tv_sub_hint);
        btnUpgrade = view.findViewById(R.id.btn_upgrade);

        rvFavorites = view.findViewById(R.id.rv_favorites);
        rvFavorites.setLayoutManager(new GridLayoutManager(getContext(), 2));
        favoritesAdapter = new MovieGridAdapter(this::openMovie);
        rvFavorites.setAdapter(favoritesAdapter);

        // Change password
        view.findViewById(R.id.tv_change_password).setOnClickListener(v ->
                Toast.makeText(getContext(), "Функция в разработке", Toast.LENGTH_SHORT).show());

        // Upgrade subscription
        btnUpgrade.setOnClickListener(v ->
                startActivity(new Intent(getContext(), SubscriptionActivity.class)));

        // Settings
        view.findViewById(R.id.btn_settings).setOnClickListener(v ->
                Toast.makeText(getContext(), "Настройки в разработке", Toast.LENGTH_SHORT).show());

        // Logout
        view.findViewById(R.id.btn_logout).setOnClickListener(v -> {
            sessionManager.logout();
            startActivity(new Intent(getContext(), LoginActivity.class));
            if (getActivity() != null) getActivity().finish();
        });

        displayLocalUser();
        loadProfileFromApi();
        loadFavorites();
    }

    private void displayLocalUser() {
        User user = sessionManager.getUser();
        if (user == null) return;

        String name = user.getName();
        tvAvatarLetter.setText(name != null && !name.isEmpty() ?
                String.valueOf(name.charAt(0)).toUpperCase() : "?");
        tvUserName.setText(name != null ? name : "Пользователь");
        tvUserEmail.setText(user.getEmail() != null ? user.getEmail() : "");

        if (user.isHasSubscription()) {
            tvSubTitle.setText(R.string.premium_subscription);
            tvSubHint.setText(R.string.active_label);
            btnUpgrade.setText(R.string.active_label);
            btnUpgrade.setBackgroundResource(R.drawable.bg_premium_badge);
        } else {
            tvSubTitle.setText(R.string.free_subscription);
            tvSubHint.setText(R.string.upgrade_hint);
            btnUpgrade.setText(R.string.upgrade_button);
        }
    }

    private void loadProfileFromApi() {
        String token = sessionManager.getToken();
        if (token == null) return;

        ApiClient.getService().getMe(token).enqueue(new Callback<ApiResponse<User>>() {
            @Override
            public void onResponse(Call<ApiResponse<User>> call, Response<ApiResponse<User>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess() && isAdded()) {
                    User serverUser = response.body().getData();
                    if (serverUser != null) {
                        sessionManager.saveUser(serverUser, token);
                        displayLocalUser();

                        // Update subscription info
                        if (serverUser.isHasSubscription() && serverUser.getSubscription() != null) {
                            SubscriptionInfo sub = serverUser.getSubscription();
                            tvSubTitle.setText(getString(R.string.premium_subscription));
                            String endDate = sub.getEndDate();
                            if (endDate != null && endDate.length() >= 10) {
                                tvSubHint.setText(String.format(getString(R.string.active_until), endDate.substring(0, 10)));
                            }
                            btnUpgrade.setText(R.string.active_label);
                            btnUpgrade.setBackgroundResource(R.drawable.bg_premium_badge);
                        }
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<User>> call, Throwable t) {}
        });
    }

    private void loadFavorites() {
        String token = sessionManager.getToken();
        if (token == null) return;

        ApiClient.getService().getFavorites(token).enqueue(new Callback<ApiResponse<List<Movie>>>() {
            @Override
            public void onResponse(Call<ApiResponse<List<Movie>>> call, Response<ApiResponse<List<Movie>>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess() && isAdded()) {
                    List<Movie> favorites = response.body().getData();
                    if (favorites != null) {
                        favoritesAdapter.setMovies(favorites);
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<Movie>>> call, Throwable t) {}
        });
    }

    private void openMovie(Movie movie) {
        Intent intent = new Intent(getContext(), FilmDetailActivity.class);
        intent.putExtra(FilmDetailActivity.EXTRA_MOVIE_ID, movie.getId());
        startActivity(intent);
    }
}

