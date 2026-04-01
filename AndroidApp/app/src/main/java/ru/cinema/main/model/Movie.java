package ru.cinema.main.model;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class Movie {
    private long id;
    private String title;
    private String description;
    @SerializedName("releaseYear")
    private Integer releaseYear;
    @SerializedName("durationMinutes")
    private Integer durationMinutes;
    @SerializedName("posterUrl")
    private String posterUrl;
    @SerializedName("bannerUrl")
    private String bannerUrl;
    private String country;
    private String director;
    @SerializedName("videoUrl")
    private String videoUrl;
    @SerializedName("vkVideoUrl")
    private String vkVideoUrl;
    @SerializedName("needSubscription")
    private boolean needSubscription;
    @SerializedName("isPublished")
    private Boolean isPublished;
    @SerializedName("averageRating")
    private double averageRating;
    @SerializedName("ratingsCount")
    private int ratingsCount;
    @SerializedName("viewCount")
    private long viewCount;
    @SerializedName("commentCount")
    private int commentCount;
    @SerializedName("createdAt")
    private String createdAt;
    @SerializedName("updatedAt")
    private String updatedAt;
    @SerializedName("publishedAt")
    private String publishedAt;
    private List<String> genres;

    // MovieDetailResponse additional fields
    @SerializedName("userRating")
    private Integer userRating;
    @SerializedName("isFavorite")
    private Boolean isFavorite;
    private List<Comment> comments;

    public Movie() {}

    public long getId() { return id; }
    public void setId(long id) { this.id = id; }
    public String getTitle() { return title; }
    public void setTitle(String title) { this.title = title; }
    public String getDescription() { return description; }
    public void setDescription(String description) { this.description = description; }
    public Integer getReleaseYear() { return releaseYear; }
    public void setReleaseYear(Integer releaseYear) { this.releaseYear = releaseYear; }
    public Integer getDurationMinutes() { return durationMinutes; }
    public void setDurationMinutes(Integer durationMinutes) { this.durationMinutes = durationMinutes; }
    public String getPosterUrl() { return posterUrl; }
    public void setPosterUrl(String posterUrl) { this.posterUrl = posterUrl; }
    public String getBannerUrl() { return bannerUrl; }
    public void setBannerUrl(String bannerUrl) { this.bannerUrl = bannerUrl; }
    public String getCountry() { return country; }
    public void setCountry(String country) { this.country = country; }
    public String getDirector() { return director; }
    public void setDirector(String director) { this.director = director; }
    public String getVideoUrl() { return videoUrl; }
    public void setVideoUrl(String videoUrl) { this.videoUrl = videoUrl; }
    public String getVkVideoUrl() { return vkVideoUrl; }
    public void setVkVideoUrl(String vkVideoUrl) { this.vkVideoUrl = vkVideoUrl; }
    public boolean isNeedSubscription() { return needSubscription; }
    public void setNeedSubscription(boolean needSubscription) { this.needSubscription = needSubscription; }
    public Boolean getIsPublished() { return isPublished; }
    public void setIsPublished(Boolean isPublished) { this.isPublished = isPublished; }
    public double getAverageRating() { return averageRating; }
    public void setAverageRating(double averageRating) { this.averageRating = averageRating; }
    public int getRatingsCount() { return ratingsCount; }
    public void setRatingsCount(int ratingsCount) { this.ratingsCount = ratingsCount; }
    public long getViewCount() { return viewCount; }
    public void setViewCount(long viewCount) { this.viewCount = viewCount; }
    public int getCommentCount() { return commentCount; }
    public void setCommentCount(int commentCount) { this.commentCount = commentCount; }
    public String getCreatedAt() { return createdAt; }
    public void setCreatedAt(String createdAt) { this.createdAt = createdAt; }
    public String getUpdatedAt() { return updatedAt; }
    public void setUpdatedAt(String updatedAt) { this.updatedAt = updatedAt; }
    public String getPublishedAt() { return publishedAt; }
    public void setPublishedAt(String publishedAt) { this.publishedAt = publishedAt; }
    public List<String> getGenres() { return genres; }
    public void setGenres(List<String> genres) { this.genres = genres; }
    public Integer getUserRating() { return userRating; }
    public void setUserRating(Integer userRating) { this.userRating = userRating; }
    public Boolean getIsFavorite() { return isFavorite; }
    public void setIsFavorite(Boolean isFavorite) { this.isFavorite = isFavorite; }
    public List<Comment> getComments() { return comments; }
    public void setComments(List<Comment> comments) { this.comments = comments; }
}
