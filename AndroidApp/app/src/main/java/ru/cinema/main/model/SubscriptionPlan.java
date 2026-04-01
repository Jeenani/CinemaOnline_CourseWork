package ru.cinema.main.model;

import com.google.gson.annotations.SerializedName;

public class SubscriptionPlan {
    private long id;
    private String name;
    private double price;
    @SerializedName("durationDays")
    private int durationDays;
    private String description;
    @SerializedName("isActive")
    private boolean isActive;

    public long getId() { return id; }
    public void setId(long id) { this.id = id; }
    public String getName() { return name; }
    public void setName(String name) { this.name = name; }
    public double getPrice() { return price; }
    public void setPrice(double price) { this.price = price; }
    public int getDurationDays() { return durationDays; }
    public void setDurationDays(int durationDays) { this.durationDays = durationDays; }
    public String getDescription() { return description; }
    public void setDescription(String description) { this.description = description; }
    public boolean isActive() { return isActive; }
    public void setActive(boolean active) { isActive = active; }
}

