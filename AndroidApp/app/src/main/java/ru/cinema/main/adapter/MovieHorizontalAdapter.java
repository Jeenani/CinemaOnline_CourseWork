package ru.cinema.main.adapter;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;

import java.util.ArrayList;
import java.util.List;

import ru.cinema.main.R;
import ru.cinema.main.api.ApiClient;
import ru.cinema.main.model.Movie;

public class MovieHorizontalAdapter extends RecyclerView.Adapter<MovieHorizontalAdapter.ViewHolder> {

    private List<Movie> movies = new ArrayList<>();
    private OnMovieClickListener listener;

    public interface OnMovieClickListener {
        void onMovieClick(Movie movie);
    }

    public MovieHorizontalAdapter(OnMovieClickListener listener) {
        this.listener = listener;
    }

    public void setMovies(List<Movie> movies) {
        this.movies = movies;
        notifyDataSetChanged();
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_movie_horizontal, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        Movie movie = movies.get(position);
        holder.bind(movie);
    }

    @Override
    public int getItemCount() {
        return movies.size();
    }

    class ViewHolder extends RecyclerView.ViewHolder {
        ImageView ivPoster;

        ViewHolder(@NonNull View itemView) {
            super(itemView);
            ivPoster = itemView.findViewById(R.id.iv_poster);
        }

        void bind(Movie movie) {
            String posterUrl = movie.getPosterUrl();
            if (posterUrl != null && !posterUrl.isEmpty()) {
                String fullUrl = posterUrl.startsWith("http") ? posterUrl : ApiClient.getBaseUrl() + posterUrl;
                Glide.with(itemView.getContext())
                        .load(fullUrl)
                        .centerCrop()
                        .into(ivPoster);
            }

            itemView.setOnClickListener(v -> {
                if (listener != null) listener.onMovieClick(movie);
            });
        }
    }
}

