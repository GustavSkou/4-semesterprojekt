import { checkCompatibility, getTotalPowerDraw, type PCComponent } from './components';

let componentId = 0;

function makeComponent(overrides: Partial<PCComponent>): PCComponent {
  return {
    id: overrides.id ?? `component-${++componentId}`,
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

describe('components compatibility requirements', () => {
  test('F02_rejects_cpu_with_incompatible_motherboard_socket', () => {
    const cpu = makeComponent({
      categoryId: 'cpu',
      name: 'Ryzen 7 7800X3D',
      socket: 'AM5',
      cpuTdp: 120,
    });
    const motherboard = makeComponent({
      categoryId: 'motherboard',
      name: 'Intel Z790 Board',
      socket: 'LGA1700',
      supportedRamTypes: ['DDR5'],
      formFactor: 'ATX',
    });

    const result = checkCompatibility(cpu, {
      cpu: null,
      motherboard,
      ram: null,
      gpu: null,
      storage: null,
      psu: null,
      case: null,
      cooling: null,
    });

    expect(result.compatible).toBe(false);
    expect(result.reasons).toContain('Requires socket AM5, motherboard has LGA1700');
  });

  test('F02_rejects_ram_with_unsupported_motherboard_type', () => {
    const ram = makeComponent({
      categoryId: 'ram',
      name: 'Legacy DDR4 Kit',
      ramType: 'DDR4',
    });
    const motherboard = makeComponent({
      categoryId: 'motherboard',
      name: 'DDR5 Board',
      supportedRamTypes: ['DDR5'],
      socket: 'AM5',
    });

    const result = checkCompatibility(ram, {
      cpu: null,
      motherboard,
      ram: null,
      gpu: null,
      storage: null,
      psu: null,
      case: null,
      cooling: null,
    });

    expect(result.compatible).toBe(false);
    expect(result.reasons).toContain('Motherboard supports DDR5, RAM is DDR4');
  });

  test('F02_rejects_component_when_total_power_exceeds_psu_capacity', () => {
    const gpu = makeComponent({
      categoryId: 'gpu',
      name: 'RTX 4090',
      powerDraw: 450,
    });
    const cpu = makeComponent({
      categoryId: 'cpu',
      name: 'Ryzen 9 7950X',
      powerDraw: 170,
      socket: 'AM5',
    });
    const psu = makeComponent({
      categoryId: 'psu',
      name: '650W PSU',
      psuWattage: 650,
    });

    const selected = {
      cpu,
      motherboard: null,
      ram: null,
      gpu: null,
      storage: null,
      psu,
      case: null,
      cooling: null,
    };

    const result = checkCompatibility(gpu, selected);

    expect(getTotalPowerDraw({ ...selected, gpu })).toBe(620);
    expect(result.compatible).toBe(true);

    const storage = makeComponent({
      categoryId: 'storage',
      name: 'Storage Array',
      powerDraw: 80,
    });
    const overloadedResult = checkCompatibility(storage, { ...selected, gpu });

    expect(overloadedResult.compatible).toBe(false);
    expect(overloadedResult.reasons).toContain('Total power draw 700W exceeds PSU capacity 650W');
  });

  test('F02_accepts_a_valid_configuration', () => {
    const cpu = makeComponent({
      categoryId: 'cpu',
      name: 'Ryzen 7 7800X3D',
      socket: 'AM5',
      powerDraw: 120,
      cpuTdp: 120,
    });
    const motherboard = makeComponent({
      categoryId: 'motherboard',
      name: 'AM5 Motherboard',
      socket: 'AM5',
      supportedRamTypes: ['DDR5'],
      formFactor: 'ATX',
    });
    const ram = makeComponent({
      categoryId: 'ram',
      name: 'DDR5 Kit',
      ramType: 'DDR5',
    });
    const cooler = makeComponent({
      categoryId: 'cooling',
      name: 'AM5 Cooler',
      supportedSockets: ['AM5'],
      maxTdp: 180,
    });
    const caseComp = makeComponent({
      categoryId: 'case',
      name: 'ATX Case',
      supportedFormFactors: ['ATX', 'mATX'],
    });
    const psu = makeComponent({
      categoryId: 'psu',
      name: '850W PSU',
      psuWattage: 850,
    });

    const selected = {
      cpu,
      motherboard,
      ram,
      gpu: null,
      storage: null,
      psu,
      case: caseComp,
      cooling: cooler,
    };

    expect(checkCompatibility(cpu, { ...selected, cpu: null })).toEqual({ compatible: true, reasons: [] });
    expect(checkCompatibility(motherboard, { ...selected, motherboard: null })).toEqual({ compatible: true, reasons: [] });
    expect(checkCompatibility(ram, { ...selected, ram: null })).toEqual({ compatible: true, reasons: [] });
  });
});
