package ru.cinema.main.model;

import com.google.gson.annotations.SerializedName;

public class SubscriptionInfo {
    private long id;
    private String name;
    @SerializedName("startDate")
    private String startDate;
    @SerializedName("endDate")
    private String endDate;

    public long getId() { return id; }
    public void setId(long id) { this.id = id; }
    public String getName() { return name; }
    public void setName(String name) { this.name = name; }
    public String getStartDate() { return startDate; }
    public void setStartDate(String startDate) { this.startDate = startDate; }
    public String getEndDate() { return endDate; }
    public void setEndDate(String endDate) { this.endDate = endDate; }
}

