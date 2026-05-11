import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { vi } from 'vitest';
import { OperatorLoginPage } from './OperatorLoginPage';

const mockNavigate = vi.fn();
const mockLogin = vi.fn();

vi.mock('react-router', async () => {
  const actual = await vi.importActual<typeof import('react-router')>('react-router');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

vi.mock('../../context/ProductionContext', () => ({
  useProduction: () => ({
    login: mockLogin,
  }),
}));

describe('OperatorLoginPage functional requirements', () => {
  beforeEach(() => {
    mockNavigate.mockReset();
    mockLogin.mockReset();
  });

  test('F12_allows_operator_login_with_valid_credentials', async () => {
    const user = userEvent.setup();
    mockLogin.mockReturnValue(true);

    render(
      <MemoryRouter>
        <OperatorLoginPage />
      </MemoryRouter>
    );

    await user.type(screen.getByLabelText('Email'), 'operator@example.com');
    await user.type(screen.getByLabelText('Password'), '1234');
    await user.click(screen.getByRole('button', { name: /login/i }));

    expect(mockLogin).toHaveBeenCalledWith('operator@example.com', '1234');
    expect(mockNavigate).toHaveBeenCalledWith('/operator/dashboard');
  });

  test('F12_rejects_operator_login_with_invalid_credentials', async () => {
    const user = userEvent.setup();
    mockLogin.mockReturnValue(false);

    render(
      <MemoryRouter>
        <OperatorLoginPage />
      </MemoryRouter>
    );

    await user.type(screen.getByLabelText('Email'), 'operator@example.com');
    await user.type(screen.getByLabelText('Password'), 'wrong-password');
    await user.click(screen.getByRole('button', { name: /login/i }));

    expect(mockLogin).toHaveBeenCalledWith('operator@example.com', 'wrong-password');
    expect(screen.getByText('Invalid credentials')).toBeInTheDocument();
    expect(mockNavigate).not.toHaveBeenCalled();
  });
});
