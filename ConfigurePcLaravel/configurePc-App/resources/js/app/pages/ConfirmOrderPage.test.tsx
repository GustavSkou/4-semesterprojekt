import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { vi } from 'vitest';
import { ConfirmOrderPage } from './ConfirmOrderPage';
import type { PCComponent } from '../data/components';

const mockNavigate = vi.fn();
const mockUseApp = vi.fn();

vi.mock('react-router', async () => {
  const actual = await vi.importActual<typeof import('react-router')>('react-router');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

vi.mock('../context/AppContext', () => ({
  useApp: () => mockUseApp(),
}));

function makeComponent(overrides: Partial<PCComponent>): PCComponent {
  return {
    id: overrides.id ?? `${overrides.categoryId}-1`,
    categoryId: overrides.categoryId ?? 'cpu',
    name: overrides.name ?? 'Test Component',
    brand: overrides.brand ?? 'Test Brand',
    price: overrides.price ?? 100,
    powerDraw: overrides.powerDraw ?? 0,
    trayId: overrides.trayId ?? 1,
    specs: overrides.specs ?? [],
    socket: overrides.socket,
    supportedSockets: overrides.supportedSockets,
    ramType: overrides.ramType,
    supportedRamTypes: overrides.supportedRamTypes,
    formFactor: overrides.formFactor,
    supportedFormFactors: overrides.supportedFormFactors,
    psuWattage: overrides.psuWattage,
    cpuTdp: overrides.cpuTdp,
    maxTdp: overrides.maxTdp,
  };
}

describe('ConfirmOrderPage functional requirements', () => {
  beforeEach(() => {
    mockNavigate.mockReset();
    mockUseApp.mockReset();
  });

  test('F01_allows_the_user_to_fill_order_inputs_and_press_the_order_button', async () => {
    const user = userEvent.setup();
    const placeOrder = vi.fn().mockResolvedValue(undefined);

    mockUseApp.mockReturnValue({
      selectedComponents: {
        cpu: makeComponent({ categoryId: 'cpu', name: 'Ryzen CPU', trayId: 10 }),
        motherboard: null,
        ram: null,
        gpu: null,
        storage: null,
        psu: null,
        case: null,
        cooling: null,
      },
      clearConfiguration: vi.fn(),
      placeOrder,
    });

    render(
      <MemoryRouter>
        <ConfirmOrderPage />
      </MemoryRouter>
    );

    await user.type(screen.getByPlaceholderText('Jane Doe'), 'Ada Lovelace');
    await user.type(screen.getByPlaceholderText('jane@example.com'), 'ada@example.com');
    await user.type(screen.getByPlaceholderText('123 Main Street, City'), '42 Computing Lane');
    await user.click(screen.getByRole('button', { name: /confirm order/i }));

    await waitFor(() => {
      expect(placeOrder).toHaveBeenCalledWith({
        name: 'Ada Lovelace',
        email: 'ada@example.com',
        address: '42 Computing Lane',
      });
    });
    expect(mockNavigate).toHaveBeenCalledWith('/order-status');
  });
});
