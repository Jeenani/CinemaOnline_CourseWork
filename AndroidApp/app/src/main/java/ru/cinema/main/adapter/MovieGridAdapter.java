package ru.cinema.main.adapter;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;

import java.util.ArrayList;
import java.util.List;

import ru.cinema.main.R;
import ru.cinema.main.api.ApiClient;
import ru.cinema.main.model.Movie;

public class MovieGridAdapter extends RecyclerView.Adapter<MovieGridAdapter.ViewHolder> {

    private List<Movie> movies = new ArrayList<>();
    private final OnMovieClickListener listener;

    public interface OnMovieClickListener {
        void onMovieClick(Movie movie);
    }

    public MovieGridAdapter(OnMovieClickListener listener) {
        this.listener = listener;
    }

    public void setMovies(List<Movie> movies) {
        this.movies = movies;
        notifyDataSetChanged();
    }

    public void addMovies(List<Movie> newMovies) {
        int start = this.movies.size();
        this.movies.addAll(newMovies);
        notifyItemRangeInserted(start, newMovies.size());
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_movie_grid, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        holder.bind(movies.get(position));
    }

    @Override
    public int getItemCount() {
        return movies.size();
    }

    class ViewHolder extends RecyclerView.ViewHolder {
        ImageView ivPoster;
        TextView tvTitle, tvYear;
        LinearLayout llStars;

        ViewHolder(@NonNull View itemView) {
            super(itemView);
            ivPoster = itemView.findViewById(R.id.iv_poster);
            tvTitle = itemView.findViewById(R.id.tv_title);
            tvYear = itemView.findViewById(R.id.tv_year);
            llStars = itemView.findViewById(R.id.ll_stars);
        }

        void bind(Movie movie) {
            tvTitle.setText(movie.getTitle());
            tvYear.setText(movie.getReleaseYear() != null ? String.valueOf(movie.getReleaseYear()) : "");

            // Poster
            String posterUrl = movie.getPosterUrl();
            if (posterUrl != null && !posterUrl.isEmpty()) {
                String fullUrl = posterUrl.startsWith("http") ? posterUrl : ApiClient.getBaseUrl() + posterUrl;
                Glide.with(itemView.getContext())
                        .load(fullUrl)
                        .centerCrop()
                        .into(ivPoster);
            }

            // Rating stars
            llStars.removeAllViews();
            int rating = (int) Math.round(movie.getAverageRating());
            for (int i = 0; i < 5; i++) {
                ImageView star = new ImageView(itemView.getContext());
                int size = (int) (12 * itemView.getContext().getResources().getDisplayMetrics().density);
                LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(size, size);
                params.setMarginEnd((int) (2 * itemView.getContext().getResources().getDisplayMetrics().density));
                star.setLayoutParams(params);
                if (i < rating) {
                    star.setImageResource(R.drawable.ic_star_filled);
                    star.setColorFilter(itemView.getContext().getColor(R.color.star_filled));
                } else {
                    star.setImageResource(R.drawable.ic_star_empty);
                    star.setColorFilter(itemView.getContext().getColor(R.color.star_empty));
                }
                llStars.addView(star);
            }

            itemView.setOnClickListener(v -> {
                if (listener != null) listener.onMovieClick(movie);
            });
        }
    }
}

