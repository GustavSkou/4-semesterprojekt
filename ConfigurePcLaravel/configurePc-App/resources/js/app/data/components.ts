export type ComponentCategory = {
  id: string;
  name: string;
};

export type PCComponent = {
  id: string;
  categoryId: string;
  name: string;
  brand: string;
  price: number;
  powerDraw: number; // watts consumed
  trayId: number;
  specs: { label: string; value: string }[];
  // Compatibility fields
  socket?: string;            // CPU, Motherboard
  supportedSockets?: string[]; // Cooler
  ramType?: string;           // RAM: 'DDR4' | 'DDR5'
  supportedRamTypes?: string[]; // Motherboard
  formFactor?: string;        // Motherboard: 'ATX' | 'mATX' | 'ITX'
  supportedFormFactors?: string[]; // Case
  psuWattage?: number;        // PSU max wattage
  cpuTdp?: number;            // CPU thermal design power
  maxTdp?: number;            // Cooler max TDP supported
};

export const categories: ComponentCategory[] = [
  { id: 'cpu', name: 'CPU' },
  { id: 'motherboard', name: 'Motherboard' },
  { id: 'ram', name: 'RAM' },
  { id: 'gpu', name: 'GPU' },
  { id: 'storage', name: 'Storage' },
  { id: 'psu', name: 'PSU' },
  { id: 'case', name: 'Case' },
  { id: 'cooling', name: 'Cooling' },
];

export function mapApiComponent(data: any): PCComponent {
  const specs = data.specifications ?? [];
  const categoryId = data.category.name.toLowerCase();
  const getSpec = (name: string) => specs.find((s: any) => s.name === name)?.value;
  const getSpecs = (name: string) => specs.filter((s: any) => s.name === name).map((s: any) => s.value);
  const isCooler = categoryId === 'cooling';

  return {
    id: String(data.id),
    categoryId,
    name: data.name,
    brand: data.brand,
    price: Number(data.price),
    powerDraw: data.wattage_lists?.[0]?.wattage ?? 0,
    trayId: data.tray_id,
    specs: specs.map((s: any) => ({ label: s.name, value: s.value })),
    socket: !isCooler ? getSpec('Socket') : undefined,
    supportedSockets: isCooler ? getSpecs('Socket') : undefined,
    ramType: getSpec('RAM Type'),
    supportedRamTypes: getSpecs('RAM Type').flatMap((v: string) => v.split(' / ')),
    formFactor: categoryId === 'motherboard' ? getSpec('Form Factor') : undefined,
    supportedFormFactors: categoryId === 'case' ? getSpecs('Form Factor') : undefined,
    psuWattage: categoryId === 'psu' ? parseInt(getSpec('Wattage') ?? '0') : undefined,
    cpuTdp: categoryId === 'cpu' ? parseInt(getSpec('TDP') ?? '0') : undefined,
    maxTdp: categoryId === 'cooling' ? parseInt(getSpec('Max TDP') ?? '0') : undefined,
  };
}

export function getTotalPowerDraw(selected: Record<string, PCComponent | null>): number {
  return Object.values(selected).reduce((sum, c) => sum + (c?.powerDraw ?? 0), 0);
}

export function checkCompatibility(
  component: PCComponent,
  selected: Record<string, PCComponent | null>
): { compatible: boolean; reasons: string[] } {
  const reasons: string[] = [];

  const cpu = selected['cpu'];
  const mb = selected['motherboard'];
  const ram = selected['ram'];
  const cooler = selected['cooling'];
  const caseComp = selected['case'];
  const psu = selected['psu'];

  switch (component.categoryId) {
    case 'cpu': {
      if (mb && mb.socket !== component.socket) {
        reasons.push(`Requires socket ${component.socket}, motherboard has ${mb.socket}`);
      }
      if (cooler && !cooler.supportedSockets?.includes(component.socket!)) {
        reasons.push(`Cooler does not support socket ${component.socket}`);
      }
      if (cooler && component.cpuTdp && cooler.maxTdp && cooler.maxTdp < component.cpuTdp) {
        reasons.push(`CPU TDP ${component.cpuTdp}W exceeds cooler max TDP ${cooler.maxTdp}W`);
      }
      break;
    }
    case 'motherboard': {
      if (cpu && cpu.socket !== component.socket) {
        reasons.push(`CPU requires socket ${cpu.socket}, this motherboard has ${component.socket}`);
      }
      if (ram && !component.supportedRamTypes?.includes(ram.ramType!)) {
        reasons.push(`RAM type ${ram.ramType} not supported — motherboard supports ${component.supportedRamTypes?.join('/')}`);
      }
      if (caseComp && !caseComp.supportedFormFactors?.includes(component.formFactor!)) {
        reasons.push(`Case does not support ${component.formFactor} form factor`);
      }
      if (cooler && cpu && !cooler.supportedSockets?.includes(component.socket!)) {
        reasons.push(`Cooler does not support socket ${component.socket}`);
      }
      break;
    }
    case 'ram': {
      if (mb && !mb.supportedRamTypes?.includes(component.ramType!)) {
        reasons.push(`Motherboard supports ${mb.supportedRamTypes?.join('/')}, RAM is ${component.ramType}`);
      }
      break;
    }
    case 'cooling': {
      if (cpu && !component.supportedSockets?.includes(cpu.socket!)) {
        reasons.push(`Cooler does not support CPU socket ${cpu.socket}`);
      }
      if (cpu && cpu.cpuTdp && component.maxTdp && component.maxTdp < cpu.cpuTdp) {
        reasons.push(`CPU TDP ${cpu.cpuTdp}W exceeds cooler max TDP ${component.maxTdp}W`);
      }
      if (mb && !component.supportedSockets?.includes(mb.socket!)) {
        reasons.push(`Cooler does not support socket ${mb.socket}`);
      }
      break;
    }
    case 'case': {
      if (mb && !component.supportedFormFactors?.includes(mb.formFactor!)) {
        reasons.push(`Case supports ${component.supportedFormFactors?.join('/')}, motherboard is ${mb.formFactor}`);
      }
      break;
    }
    case 'psu': {
      const totalWithThisRemoved = getTotalPowerDraw({ ...selected, psu: null });
      if (component.psuWattage && component.psuWattage < totalWithThisRemoved) {
        reasons.push(`${component.psuWattage}W PSU is insufficient — build draws ${totalWithThisRemoved}W`);
      }
      break;
    }
    default:
      break;
  }

  // General PSU check for non-PSU components
  if (component.categoryId !== 'psu' && psu) {
    const totalWithNew = getTotalPowerDraw({ ...selected, [component.categoryId]: component });
    if (psu.psuWattage && psu.psuWattage < totalWithNew) {
      reasons.push(`Total power draw ${totalWithNew}W exceeds PSU capacity ${psu.psuWattage}W`);
    }
  }

  return { compatible: reasons.length === 0, reasons };
}
