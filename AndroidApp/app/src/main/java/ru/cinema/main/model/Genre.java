package ru.cinema.main.model;

import com.google.gson.annotations.SerializedName;

public class Genre {
    private long id;
    private String name;
    @SerializedName("displayName")
    private String displayName;

    public long getId() { return id; }
    public void setId(long id) { this.id = id; }
    public String getName() { return name; }
    public void setName(String name) { this.name = name; }
    public String getDisplayName() { return displayName; }
    public void setDisplayName(String displayName) { this.displayName = displayName; }
}

