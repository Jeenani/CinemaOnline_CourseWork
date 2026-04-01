package ru.cinema.main;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import java.util.HashMap;
import java.util.Map;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import ru.cinema.main.api.ApiClient;
import ru.cinema.main.model.ApiResponse;
import ru.cinema.main.model.AuthResponse;
import ru.cinema.main.util.SessionManager;

public class LoginActivity extends AppCompatActivity {

    private EditText etEmail, etPassword;
    private TextView tvError, tvGoRegister;
    private Button btnLogin;
    private SessionManager sessionManager;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        sessionManager = new SessionManager(this);

        if (sessionManager.isLoggedIn()) {
            goToMain();
            return;
        }

        etEmail = findViewById(R.id.et_email);
        etPassword = findViewById(R.id.et_password);
        tvError = findViewById(R.id.tv_error);
        tvGoRegister = findViewById(R.id.tv_go_register);
        btnLogin = findViewById(R.id.btn_login);

        btnLogin.setOnClickListener(v -> doLogin());

        tvGoRegister.setOnClickListener(v -> {
            startActivity(new Intent(this, RegisterActivity.class));
            finish();
        });
    }

    private void doLogin() {
        String email = etEmail.getText().toString().trim();
        String password = etPassword.getText().toString().trim();

        if (email.isEmpty() || password.isEmpty()) {
            showError(getString(R.string.error_fill_fields));
            return;
        }

        btnLogin.setEnabled(false);
        Map<String, String> body = new HashMap<>();
        body.put("email", email);
        body.put("password", password);

        ApiClient.getService().login(body).enqueue(new Callback<ApiResponse<AuthResponse>>() {
            @Override
            public void onResponse(Call<ApiResponse<AuthResponse>> call, Response<ApiResponse<AuthResponse>> response) {
                btnLogin.setEnabled(true);
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    AuthResponse auth = response.body().getData();
                    sessionManager.saveUser(auth.getUser(), auth.getToken());
                    goToMain();
                } else {
                    showError(getString(R.string.error_invalid_credentials));
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<AuthResponse>> call, Throwable t) {
                btnLogin.setEnabled(true);
                showError(String.format(getString(R.string.error_network), t.getMessage()));
            }
        });
    }

    private void showError(String msg) {
        tvError.setText(msg);
        tvError.setVisibility(View.VISIBLE);
    }

    private void goToMain() {
        startActivity(new Intent(this, MainActivity.class));
        finish();
    }
}
