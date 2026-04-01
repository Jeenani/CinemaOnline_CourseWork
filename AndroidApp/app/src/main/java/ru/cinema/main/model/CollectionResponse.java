package ru.cinema.main.model;

import java.util.List;

public class CollectionResponse {
    private long id;
    private String name;
    private String description;
    private List<Movie> movies;

    public CollectionResponse() {}

    public long getId() { return id; }
    public void setId(long id) { this.id = id; }
    public String getName() { return name; }
    public void setName(String name) { this.name = name; }
    public String getDescription() { return description; }
    public void setDescription(String description) { this.description = description; }
    public List<Movie> getMovies() { return movies; }
    public void setMovies(List<Movie> movies) { this.movies = movies; }
}
