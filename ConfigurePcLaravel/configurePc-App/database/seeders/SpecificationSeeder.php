<?php

namespace Database\Seeders;

use App\Models\Component;
use App\Models\Specification;
use Illuminate\Database\Seeder;

class SpecificationSeeder extends Seeder
{
    public function run(): void
    {
        // tray_id => [ [name, value], ... ]
        $specs = [
            // CPU
            1 => [
                ['name' => 'Socket',          'value' => 'AM5'],
                ['name' => 'Cores / Threads', 'value' => '6C / 12T'],
                ['name' => 'Base / Boost',    'value' => '4.7 / 5.3 GHz'],
                ['name' => 'RAM Support',     'value' => 'DDR5'],
                ['name' => 'TDP',             'value' => '105W'],
            ],
            2 => [
                ['name' => 'Socket',          'value' => 'AM5'],
                ['name' => 'Cores / Threads', 'value' => '8C / 16T'],
                ['name' => 'Base / Boost',    'value' => '4.2 / 5.0 GHz'],
                ['name' => 'RAM Support',     'value' => 'DDR5'],
                ['name' => 'TDP',             'value' => '120W'],
            ],
            3 => [
                ['name' => 'Socket',          'value' => 'LGA1700'],
                ['name' => 'Cores / Threads', 'value' => '14C / 20T'],
                ['name' => 'Base / Boost',    'value' => '3.5 / 5.3 GHz'],
                ['name' => 'RAM Support',     'value' => 'DDR4 / DDR5'],
                ['name' => 'TDP',             'value' => '125W'],
            ],
            4 => [
                ['name' => 'Socket',          'value' => 'LGA1700'],
                ['name' => 'Cores / Threads', 'value' => '24C / 32T'],
                ['name' => 'Base / Boost',    'value' => '3.2 / 6.0 GHz'],
                ['name' => 'RAM Support',     'value' => 'DDR4 / DDR5'],
                ['name' => 'TDP',             'value' => '253W'],
            ],
            // Motherboard
            5 => [
                ['name' => 'Socket',      'value' => 'AM5'],
                ['name' => 'Form Factor', 'value' => 'ATX'],
                ['name' => 'RAM Type',    'value' => 'DDR5'],
                ['name' => 'RAM Slots',   'value' => '4x DDR5'],
                ['name' => 'Max RAM',     'value' => '128GB'],
                ['name' => 'PCIe',        'value' => 'x16 5.0'],
            ],
            6 => [
                ['name' => 'Socket',      'value' => 'AM5'],
                ['name' => 'Form Factor', 'value' => 'mATX'],
                ['name' => 'RAM Type',    'value' => 'DDR5'],
                ['name' => 'RAM Slots',   'value' => '4x DDR5'],
                ['name' => 'Max RAM',     'value' => '128GB'],
                ['name' => 'PCIe',        'value' => 'x16 4.0'],
            ],
            7 => [
                ['name' => 'Socket',      'value' => 'LGA1700'],
                ['name' => 'Form Factor', 'value' => 'ATX'],
                ['name' => 'RAM Type',    'value' => 'DDR5'],
                ['name' => 'RAM Slots',   'value' => '4x DDR5'],
                ['name' => 'Max RAM',     'value' => '192GB'],
                ['name' => 'PCIe',        'value' => 'x16 5.0'],
            ],
            8 => [
                ['name' => 'Socket',      'value' => 'LGA1700'],
                ['name' => 'Form Factor', 'value' => 'mATX'],
                ['name' => 'RAM Type',    'value' => 'DDR4 / DDR5'],
                ['name' => 'RAM Slots',   'value' => '4x DDR4/DDR5'],
                ['name' => 'Max RAM',     'value' => '128GB'],
                ['name' => 'PCIe',        'value' => 'x16 4.0'],
            ],
            // RAM
            9 => [
                ['name' => 'RAM Type', 'value' => 'DDR5'],
                ['name' => 'Speed',    'value' => '6000MHz'],
                ['name' => 'Capacity', 'value' => '2x16GB'],
                ['name' => 'Latency',  'value' => 'CL30'],
            ],
            10 => [
                ['name' => 'RAM Type', 'value' => 'DDR5'],
                ['name' => 'Speed',    'value' => '7200MHz'],
                ['name' => 'Capacity', 'value' => '2x16GB'],
                ['name' => 'Latency',  'value' => 'CL34'],
            ],
            11 => [
                ['name' => 'RAM Type', 'value' => 'DDR4'],
                ['name' => 'Speed',    'value' => '3600MHz'],
                ['name' => 'Capacity', 'value' => '2x16GB'],
                ['name' => 'Latency',  'value' => 'CL18'],
            ],
            12 => [
                ['name' => 'RAM Type', 'value' => 'DDR4'],
                ['name' => 'Speed',    'value' => '3200MHz'],
                ['name' => 'Capacity', 'value' => '2x8GB'],
                ['name' => 'Latency',  'value' => 'CL16'],
            ],
            // GPU
            13 => [
                ['name' => 'VRAM',        'value' => '8GB GDDR6'],
                ['name' => 'Boost Clock', 'value' => '2460MHz'],
                ['name' => 'TDP',         'value' => '115W'],
                ['name' => 'Outputs',     'value' => 'HDMI 2.1, 3x DP'],
            ],
            14 => [
                ['name' => 'VRAM',        'value' => '12GB GDDR6X'],
                ['name' => 'Boost Clock', 'value' => '2475MHz'],
                ['name' => 'TDP',         'value' => '200W'],
                ['name' => 'Outputs',     'value' => 'HDMI 2.1, 3x DP'],
            ],
            15 => [
                ['name' => 'VRAM',        'value' => '16GB GDDR6'],
                ['name' => 'Boost Clock', 'value' => '2430MHz'],
                ['name' => 'TDP',         'value' => '263W'],
                ['name' => 'Outputs',     'value' => 'HDMI 2.1, 2x DP, USB-C'],
            ],
            16 => [
                ['name' => 'VRAM',        'value' => '24GB GDDR6X'],
                ['name' => 'Boost Clock', 'value' => '2520MHz'],
                ['name' => 'TDP',         'value' => '450W'],
                ['name' => 'Outputs',     'value' => 'HDMI 2.1, 3x DP'],
            ],
            // Storage
            17 => [
                ['name' => 'Type',       'value' => 'NVMe PCIe 4.0'],
                ['name' => 'Capacity',   'value' => '1TB'],
                ['name' => 'Read/Write', 'value' => '7450 / 6900 MB/s'],
            ],
            18 => [
                ['name' => 'Type',       'value' => 'NVMe PCIe 4.0'],
                ['name' => 'Capacity',   'value' => '2TB'],
                ['name' => 'Read/Write', 'value' => '7300 / 6600 MB/s'],
            ],
            19 => [
                ['name' => 'Type',     'value' => 'SATA HDD'],
                ['name' => 'Capacity', 'value' => '4TB'],
                ['name' => 'Speed',    'value' => '7200 RPM'],
            ],
            // PSU
            20 => [
                ['name' => 'Wattage',    'value' => '650W'],
                ['name' => 'Efficiency', 'value' => '80+ Gold'],
                ['name' => 'Modular',    'value' => 'Fully Modular'],
            ],
            21 => [
                ['name' => 'Wattage',    'value' => '750W'],
                ['name' => 'Efficiency', 'value' => '80+ Gold'],
                ['name' => 'Modular',    'value' => 'Fully Modular'],
            ],
            22 => [
                ['name' => 'Wattage',    'value' => '850W'],
                ['name' => 'Efficiency', 'value' => '80+ Gold'],
                ['name' => 'Modular',    'value' => 'Fully Modular'],
            ],
            23 => [
                ['name' => 'Wattage',    'value' => '1000W'],
                ['name' => 'Efficiency', 'value' => '80+ Platinum'],
                ['name' => 'Modular',    'value' => 'Fully Modular'],
            ],
            // Case
            24 => [
                ['name' => 'Form Factor', 'value' => 'ATX'],
                ['name' => 'Form Factor', 'value' => 'mATX'],
                ['name' => 'Drive Bays',  'value' => '2x 3.5" + 3x 2.5"'],
                ['name' => 'Fan Support', 'value' => 'Up to 360mm rad'],
            ],
            25 => [
                ['name' => 'Form Factor', 'value' => 'ATX'],
                ['name' => 'Drive Bays',  'value' => '6x 2.5"'],
                ['name' => 'Fan Support', 'value' => 'Up to 360mm rad x2'],
            ],
            26 => [
                ['name' => 'Form Factor', 'value' => 'ATX'],
                ['name' => 'Form Factor', 'value' => 'mATX'],
                ['name' => 'Drive Bays',  'value' => '2x 3.5" + 2x 2.5"'],
                ['name' => 'Fan Support', 'value' => 'Up to 240mm rad'],
            ],
            27 => [
                ['name' => 'Form Factor', 'value' => 'mATX'],
                ['name' => 'Form Factor', 'value' => 'ITX'],
                ['name' => 'Drive Bays',  'value' => '1x 3.5" + 2x 2.5"'],
                ['name' => 'Fan Support', 'value' => 'Up to 280mm rad'],
            ],
            // Cooling
            28 => [
                ['name' => 'Type',     'value' => 'Air Cooler'],
                ['name' => 'Fan Size', 'value' => '2x 140mm'],
                ['name' => 'Max TDP',  'value' => '250W'],
                ['name' => 'Socket',   'value' => 'AM4'],
                ['name' => 'Socket',   'value' => 'AM5'],
                ['name' => 'Socket',   'value' => 'LGA1700'],
            ],
            29 => [
                ['name' => 'Type',     'value' => 'Air Cooler'],
                ['name' => 'Fan Size', 'value' => '1x 135mm + 1x 120mm'],
                ['name' => 'Max TDP',  'value' => '250W'],
                ['name' => 'Socket',   'value' => 'AM4'],
                ['name' => 'Socket',   'value' => 'AM5'],
                ['name' => 'Socket',   'value' => 'LGA1700'],
            ],
            30 => [
                ['name' => 'Type',     'value' => '360mm AIO Liquid'],
                ['name' => 'Fan Size', 'value' => '3x 120mm'],
                ['name' => 'Max TDP',  'value' => '350W'],
                ['name' => 'Socket',   'value' => 'AM4'],
                ['name' => 'Socket',   'value' => 'AM5'],
                ['name' => 'Socket',   'value' => 'LGA1700'],
            ],
            31 => [
                ['name' => 'Type',     'value' => 'Air Cooler'],
                ['name' => 'Fan Size', 'value' => '1x 120mm'],
                ['name' => 'Max TDP',  'value' => '220W'],
                ['name' => 'Socket',   'value' => 'AM4'],
                ['name' => 'Socket',   'value' => 'AM5'],
                ['name' => 'Socket',   'value' => 'LGA1700'],
            ],
        ];

        foreach ($specs as $trayId => $rows) {
            $component = Component::where('tray_id', $trayId)->firstOrFail();
            foreach ($rows as $row) {
                Specification::create([
                    'component_id' => $component->id,
                    'name'         => $row['name'],
                    'value'        => $row['value'],
                ]);
            }
        }
    }
}
