import React, { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  KeyboardAvoidingView,
  Platform,
  ActivityIndicator,
  Alert,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useAuth } from '../../context/AuthContext';
import { authApi } from '../../services/api';
import { colors } from '../../theme/colors';
import { ENABLE_DEMO_LOGIN } from '../../config/api';

export default function LoginScreen({ navigation }: { navigation: any }) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();

  const handleLogin = async () => {
    if (!email.trim() || !password) {
      Alert.alert('Hata', 'E-posta ve şifre gerekli');
      return;
    }
    setLoading(true);
    try {
      const { data } = await authApi.login(email.trim(), password);
      await login(data.token, {
        id: data.user.id,
        email: data.user.email,
        name: data.user.name,
      });
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Giriş başarısız';
      Alert.alert('Giriş Hatası', msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <SafeAreaView style={styles.container} edges={['top']}>
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        style={styles.keyboard}
      >
        <View style={styles.header}>
          <Text style={styles.title}>Tekrar Hoş Geldin</Text>
          <Text style={styles.subtitle}>Hesabına giriş yap, antrenmanına kaldığın yerden devam et.</Text>
        </View>

        <View style={styles.form}>
          <TextInput
            style={styles.input}
            placeholder="E-posta"
            placeholderTextColor={colors.textSecondary}
            value={email}
            onChangeText={setEmail}
            keyboardType="email-address"
            autoCapitalize="none"
            autoCorrect={false}
          />
          <TextInput
            style={styles.input}
            placeholder="Şifre"
            placeholderTextColor={colors.textSecondary}
            value={password}
            onChangeText={setPassword}
            secureTextEntry
          />

          <TouchableOpacity
            style={[styles.button, loading && styles.buttonDisabled]}
            onPress={handleLogin}
            disabled={loading}
          >
            {loading ? (
              <ActivityIndicator color={colors.white} />
            ) : (
              <Text style={styles.buttonText}>Giriş Yap</Text>
            )}
          </TouchableOpacity>

          <TouchableOpacity
            style={styles.link}
            onPress={() => navigation.navigate('Register')}
          >
            <Text style={styles.linkText}>
              Hesabın yok mu? <Text style={styles.linkTextBold}>Kayıt ol</Text>
            </Text>
          </TouchableOpacity>

          {ENABLE_DEMO_LOGIN && (
            <TouchableOpacity
              style={styles.demoButton}
              onPress={() => login('demo-token', { id: 'demo', email: 'demo@test.com', name: 'Demo Kullanıcı' })}
            >
              <Text style={styles.demoButtonText}>Demo ile giriş (backend yokken test)</Text>
            </TouchableOpacity>
          )}
        </View>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  keyboard: {
    flex: 1,
    paddingHorizontal: 24,
    justifyContent: 'center',
  },
  header: {
    marginBottom: 32,
  },
  title: {
    fontSize: 28,
    fontWeight: '700',
    color: colors.text,
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 16,
    color: colors.textSecondary,
    lineHeight: 22,
  },
  form: {
    gap: 16,
  },
  input: {
    backgroundColor: colors.surface,
    borderRadius: 12,
    paddingHorizontal: 16,
    paddingVertical: 14,
    fontSize: 16,
    color: colors.text,
    borderWidth: 1,
    borderColor: colors.surfaceLight,
  },
  button: {
    backgroundColor: colors.primary,
    borderRadius: 12,
    paddingVertical: 16,
    alignItems: 'center',
    marginTop: 8,
  },
  buttonDisabled: {
    opacity: 0.7,
  },
  buttonText: {
    color: colors.white,
    fontSize: 16,
    fontWeight: '600',
  },
  link: {
    alignItems: 'center',
    marginTop: 16,
  },
  linkText: {
    color: colors.textSecondary,
    fontSize: 15,
  },
  linkTextBold: {
    color: colors.primary,
    fontWeight: '600',
  },
  demoButton: {
    marginTop: 24,
    paddingVertical: 12,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: colors.surfaceLight,
    borderRadius: 12,
  },
  demoButtonText: {
    color: colors.textSecondary,
    fontSize: 14,
  },
});
