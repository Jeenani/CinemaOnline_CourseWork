package ru.cinema.main;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.TextView;

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

public class RegisterActivity extends AppCompatActivity {

    private EditText etName, etEmail, etPassword, etConfirmPassword;
    private TextView tvError, tvGoLogin;
    private TextView btnRegister;
    private SessionManager sessionManager;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_register);

        sessionManager = new SessionManager(this);

        etName = findViewById(R.id.et_name);
        etEmail = findViewById(R.id.et_email);
        etPassword = findViewById(R.id.et_password);
        etConfirmPassword = findViewById(R.id.et_confirm_password);
        tvError = findViewById(R.id.tv_error);
        tvGoLogin = findViewById(R.id.tv_login_link);
        btnRegister = findViewById(R.id.btn_register);

        btnRegister.setOnClickListener(v -> doRegister());

        tvGoLogin.setOnClickListener(v -> {
            startActivity(new Intent(this, LoginActivity.class));
            finish();
        });
    }

    private void doRegister() {
        String name = etName.getText().toString().trim();
        String email = etEmail.getText().toString().trim();
        String password = etPassword.getText().toString().trim();
        String confirmPassword = etConfirmPassword.getText().toString().trim();

        if (name.isEmpty() || email.isEmpty() || password.isEmpty() || confirmPassword.isEmpty()) {
            showError(getString(R.string.error_fill_fields));
            return;
        }

        if (!password.equals(confirmPassword)) {
            showError(getString(R.string.error_passwords_mismatch));
            return;
        }

        if (password.length() < 6) {
            showError(getString(R.string.error_password_short));
            return;
        }

        btnRegister.setEnabled(false);
        Map<String, String> body = new HashMap<>();
        body.put("name", name);
        body.put("email", email);
        body.put("password", password);

        ApiClient.getService().register(body).enqueue(new Callback<ApiResponse<AuthResponse>>() {
            @Override
            public void onResponse(Call<ApiResponse<AuthResponse>> call, Response<ApiResponse<AuthResponse>> response) {
                btnRegister.setEnabled(true);
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    AuthResponse auth = response.body().getData();
                    sessionManager.saveUser(auth.getUser(), auth.getToken());
                    startActivity(new Intent(RegisterActivity.this, MainActivity.class));
                    finish();
                } else {
                    showError("Ошибка регистрации. Возможно, email уже занят.");
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<AuthResponse>> call, Throwable t) {
                btnRegister.setEnabled(true);
                showError("Ошибка сети: " + t.getMessage());
            }
        });
    }

    private void showError(String msg) {
        tvError.setText(msg);
        tvError.setVisibility(View.VISIBLE);
    }
}

