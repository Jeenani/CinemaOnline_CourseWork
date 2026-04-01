package ru.cinema.main.api;

import java.util.List;
import java.util.Map;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.DELETE;
import retrofit2.http.GET;
import retrofit2.http.Header;
import retrofit2.http.POST;
import retrofit2.http.Path;
import retrofit2.http.Query;
import ru.cinema.main.model.ApiResponse;
import ru.cinema.main.model.AuthResponse;
import ru.cinema.main.model.CollectionResponse;
import ru.cinema.main.model.Comment;
import ru.cinema.main.model.Genre;
import ru.cinema.main.model.Movie;
import ru.cinema.main.model.PagedResponse;
import ru.cinema.main.model.SubscriptionPlan;
import ru.cinema.main.model.User;

public interface ApiService {

    // ========================
    // AUTH
    // ========================

    @POST("api/auth/register")
    Call<ApiResponse<AuthResponse>> register(@Body Map<String, String> body);

    @POST("api/auth/login")
    Call<ApiResponse<AuthResponse>> login(@Body Map<String, String> body);

    @GET("api/auth/me")
    Call<ApiResponse<User>> getMe(@Header("Authorization") String token);

    @POST("api/auth/update-profile")
    Call<ApiResponse<User>> updateProfile(
            @Header("Authorization") String token,
            @Body Map<String, String> body
    );

    // ========================
    // MOVIES
    // ========================

    @GET("api/movies")
    Call<ApiResponse<PagedResponse<Movie>>> getMovies(
            @Query("search") String search,
            @Query("genreId") Long genreId,
            @Query("yearFrom") Integer yearFrom,
            @Query("yearTo") Integer yearTo,
            @Query("sortBy") String sortBy,
            @Query("sortDescending") Boolean sortDescending,
            @Query("page") Integer page,
            @Query("pageSize") Integer pageSize
    );

    @GET("api/movies/random")
    Call<ApiResponse<Movie>> getRandomMovie();

    @GET("api/movies/{id}")
    Call<ApiResponse<Movie>> getMovie(@Path("id") long id);

    @GET("api/movies/{id}")
    Call<ApiResponse<Movie>> getMovieAuth(
            @Path("id") long id,
            @Header("Authorization") String token
    );

    // ========================
    // COMMENTS
    // ========================

    @GET("api/movies/{id}/comments")
    Call<ApiResponse<List<Comment>>> getComments(@Path("id") long movieId);

    @POST("api/movies/{id}/comments")
    Call<ApiResponse<Long>> addComment(
            @Path("id") long movieId,
            @Header("Authorization") String token,
            @Body Map<String, String> body
    );

    // ========================
    // RATINGS
    // ========================

    @POST("api/movies/{id}/rate")
    Call<ApiResponse<Boolean>> rateMovie(
            @Path("id") long movieId,
            @Header("Authorization") String token,
            @Body Map<String, Integer> body
    );

    // ========================
    // FAVORITES
    // ========================

    @POST("api/movies/{id}/favorite")
    Call<ApiResponse<Boolean>> addFavorite(
            @Path("id") long movieId,
            @Header("Authorization") String token
    );

    @DELETE("api/movies/{id}/favorite")
    Call<ApiResponse<Boolean>> removeFavorite(
            @Path("id") long movieId,
            @Header("Authorization") String token
    );

    // ========================
    // USER
    // ========================

    @GET("api/user/favorites")
    Call<ApiResponse<List<Movie>>> getFavorites(@Header("Authorization") String token);

    // ========================
    // GENRES
    // ========================

    @GET("api/genres")
    Call<ApiResponse<List<Genre>>> getGenres();

    // ========================
    // COLLECTIONS
    // ========================

    @GET("api/collections/featured")
    Call<ApiResponse<List<CollectionResponse>>> getFeaturedCollections();

    // ========================
    // SUBSCRIPTIONS
    // ========================

    @GET("api/subscriptions")
    Call<ApiResponse<List<SubscriptionPlan>>> getSubscriptionPlans();

    // ========================
    // PAYMENTS
    // ========================

    @POST("api/payments")
    Call<ApiResponse<Map<String, Object>>> createPayment(
            @Header("Authorization") String token,
            @Body Map<String, Object> body
    );

    @POST("api/payments/{id}/process")
    Call<ApiResponse<Boolean>> processPayment(
            @Path("id") long paymentId,
            @Body Map<String, Object> body
    );

    @GET("api/payments/my")
    Call<ApiResponse<List<Map<String, Object>>>> getMyPayments(
            @Header("Authorization") String token
    );

    // ========================
    // VIEW HISTORY
    // ========================

    @POST("api/movies/{id}/view")
    Call<ApiResponse<Boolean>> recordView(
            @Path("id") long movieId,
            @Header("Authorization") String token,
            @Body Map<String, Object> body
    );

    @GET("api/user/history")
    Call<ApiResponse<List<Movie>>> getHistory(@Header("Authorization") String token);
}
