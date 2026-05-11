import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { vi } from 'vitest';
import { ConfigurePage } from './ConfigurePage';
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

describe('ConfigurePage functional requirements', () => {
  beforeEach(() => {
    mockNavigate.mockReset();
    mockUseApp.mockReset();
  });

  test('F02_blocks_selection_of_incompatible_components_in_the_gui', async () => {
    const user = userEvent.setup();
    const cpu = makeComponent({
      categoryId: 'cpu',
      name: 'Ryzen CPU',
      socket: 'AM5',
      price: 320,
      trayId: 10,
    });
    const incompatibleMotherboard = makeComponent({
      id: 'mb-intel',
      categoryId: 'motherboard',
      name: 'Intel Motherboard',
      socket: 'LGA1700',
      supportedRamTypes: ['DDR5'],
      formFactor: 'ATX',
      price: 180,
      trayId: 11,
    });
    const compatibleMotherboard = makeComponent({
      id: 'mb-am5',
      categoryId: 'motherboard',
      name: 'AM5 Motherboard',
      socket: 'AM5',
      supportedRamTypes: ['DDR5'],
      formFactor: 'ATX',
      price: 210,
      trayId: 12,
    });
    const selectComponent = vi.fn();

    mockUseApp.mockReturnValue({
      selectedComponents: {
        cpu,
        motherboard: null,
        ram: null,
        gpu: null,
        storage: null,
        psu: null,
        case: null,
        cooling: null,
      },
      selectComponent,
      deselectComponent: vi.fn(),
      clearConfiguration: vi.fn(),
      components: [cpu, incompatibleMotherboard, compatibleMotherboard],
    });

    render(
      <MemoryRouter>
        <ConfigurePage />
      </MemoryRouter>
    );

    await user.click(screen.getByRole('button', { name: /motherboard/i }));
    await user.click(screen.getByText('Intel Motherboard'));
    expect(selectComponent).not.toHaveBeenCalled();

    await user.click(screen.getByText('AM5 Motherboard'));
    expect(selectComponent).toHaveBeenCalledWith(compatibleMotherboard);
  });
});
