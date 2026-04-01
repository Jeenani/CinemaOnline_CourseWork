package ru.cinema.main.adapter;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import java.util.ArrayList;
import java.util.List;

import ru.cinema.main.R;
import ru.cinema.main.model.Comment;

public class CommentAdapter extends RecyclerView.Adapter<CommentAdapter.ViewHolder> {

    private List<Comment> comments = new ArrayList<>();

    public void setComments(List<Comment> comments) {
        this.comments = comments;
        notifyDataSetChanged();
    }

    public void addComment(Comment comment) {
        this.comments.add(0, comment);
        notifyItemInserted(0);
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_comment, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        holder.bind(comments.get(position));
    }

    @Override
    public int getItemCount() {
        return comments.size();
    }

    static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvAvatar, tvAuthor, tvText;
        LinearLayout llStars;

        ViewHolder(@NonNull View itemView) {
            super(itemView);
            tvAvatar = itemView.findViewById(R.id.tv_avatar);
            tvAuthor = itemView.findViewById(R.id.tv_author);
            tvText = itemView.findViewById(R.id.tv_text);
            llStars = itemView.findViewById(R.id.ll_stars);
        }

        void bind(Comment comment) {
            String name = comment.getUserName();
            tvAvatar.setText(name != null && !name.isEmpty() ? String.valueOf(name.charAt(0)).toUpperCase() : "?");
            tvAuthor.setText(name != null ? name : "Аноним");
            tvText.setText(comment.getContent());

            llStars.removeAllViews();
            // Comments don't have rating from API, but we can show empty stars placeholder
        }
    }
}
