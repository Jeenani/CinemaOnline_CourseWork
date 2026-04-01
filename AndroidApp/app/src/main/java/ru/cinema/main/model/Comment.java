package ru.cinema.main.model;

import com.google.gson.annotations.SerializedName;

public class Comment {
    private long id;
    @SerializedName("userId")
    private long userId;
    @SerializedName("userName")
    private String userName;
    private String content;
    @SerializedName("createdAt")
    private String createdAt;

    public Comment() {}

    public long getId() { return id; }
    public void setId(long id) { this.id = id; }
    public long getUserId() { return userId; }
    public void setUserId(long userId) { this.userId = userId; }
    public String getUserName() { return userName; }
    public void setUserName(String userName) { this.userName = userName; }
    public String getContent() { return content; }
    public void setContent(String content) { this.content = content; }
    public String getCreatedAt() { return createdAt; }
    public void setCreatedAt(String createdAt) { this.createdAt = createdAt; }
}

