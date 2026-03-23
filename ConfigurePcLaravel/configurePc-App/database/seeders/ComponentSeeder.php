<?php

namespace Database\Seeders;

use App\Models\Category;
use App\Models\Component;
use Illuminate\Database\Seeder;

class ComponentSeeder extends Seeder
{
    public function run(): void
    {
        $categories = [
            'cpu'         => Category::create(['name' => 'CPU']),
            'motherboard' => Category::create(['name' => 'Motherboard']),
            'ram'         => Category::create(['name' => 'RAM']),
            'gpu'         => Category::create(['name' => 'GPU']),
            'storage'     => Category::create(['name' => 'Storage']),
            'psu'         => Category::create(['name' => 'PSU']),
            'case'        => Category::create(['name' => 'Case']),
            'cooling'     => Category::create(['name' => 'Cooling']),
        ];

        $components = [
            // CPU
            ['name' => 'Ryzen 5 7600X',           'brand' => 'AMD',            'category' => 'cpu',         'price' => 229,  'tray_id' => 1],
            ['name' => 'Ryzen 7 7800X3D',          'brand' => 'AMD',            'category' => 'cpu',         'price' => 389,  'tray_id' => 2],
            ['name' => 'Core i5-14600K',            'brand' => 'Intel',          'category' => 'cpu',         'price' => 259,  'tray_id' => 3],
            ['name' => 'Core i9-14900K',            'brand' => 'Intel',          'category' => 'cpu',         'price' => 529,  'tray_id' => 4],
            // Motherboard
            ['name' => 'ROG Strix B650E-F',        'brand' => 'ASUS',           'category' => 'motherboard', 'price' => 299,  'tray_id' => 5],
            ['name' => 'MAG B650M Mortar',          'brand' => 'MSI',            'category' => 'motherboard', 'price' => 189,  'tray_id' => 6],
            ['name' => 'ROG Maximus Z790 Hero',     'brand' => 'ASUS',           'category' => 'motherboard', 'price' => 499,  'tray_id' => 7],
            ['name' => 'B760M DS3H',                'brand' => 'Gigabyte',       'category' => 'motherboard', 'price' => 139,  'tray_id' => 8],
            // RAM
            ['name' => 'Vengeance DDR5-6000 32GB',  'brand' => 'Corsair',        'category' => 'ram',         'price' => 89,   'tray_id' => 9],
            ['name' => 'Trident Z5 DDR5-7200 32GB', 'brand' => 'G.Skill',        'category' => 'ram',         'price' => 129,  'tray_id' => 10],
            ['name' => 'Vengeance DDR4-3600 32GB',  'brand' => 'Corsair',        'category' => 'ram',         'price' => 65,   'tray_id' => 11],
            ['name' => 'Fury Beast DDR4-3200 16GB', 'brand' => 'Kingston',       'category' => 'ram',         'price' => 45,   'tray_id' => 12],
            // GPU
            ['name' => 'GeForce RTX 4060',          'brand' => 'NVIDIA',         'category' => 'gpu',         'price' => 299,  'tray_id' => 13],
            ['name' => 'GeForce RTX 4070',          'brand' => 'NVIDIA',         'category' => 'gpu',         'price' => 599,  'tray_id' => 14],
            ['name' => 'Radeon RX 7800 XT',         'brand' => 'AMD',            'category' => 'gpu',         'price' => 499,  'tray_id' => 15],
            ['name' => 'GeForce RTX 4090',          'brand' => 'NVIDIA',         'category' => 'gpu',         'price' => 1599, 'tray_id' => 16],
            // Storage
            ['name' => '990 Pro 1TB NVMe',          'brand' => 'Samsung',        'category' => 'storage',     'price' => 99,   'tray_id' => 17],
            ['name' => 'Black SN850X 2TB',          'brand' => 'WD',             'category' => 'storage',     'price' => 169,  'tray_id' => 18],
            ['name' => 'Barracuda 4TB HDD',         'brand' => 'Seagate',        'category' => 'storage',     'price' => 79,   'tray_id' => 19],
            // PSU
            ['name' => 'Focus GX 650W',             'brand' => 'Seasonic',       'category' => 'psu',         'price' => 99,   'tray_id' => 20],
            ['name' => 'RM750 750W',                'brand' => 'Corsair',        'category' => 'psu',         'price' => 119,  'tray_id' => 21],
            ['name' => 'SuperNOVA 850W G6',         'brand' => 'EVGA',           'category' => 'psu',         'price' => 149,  'tray_id' => 22],
            ['name' => 'Straight Power 1000W',      'brand' => 'be quiet!',      'category' => 'psu',         'price' => 199,  'tray_id' => 23],
            // Case
            ['name' => 'Meshify C',                 'brand' => 'Fractal Design', 'category' => 'case',        'price' => 99,   'tray_id' => 24],
            ['name' => 'PC-O11 Dynamic',            'brand' => 'Lian Li',        'category' => 'case',        'price' => 149,  'tray_id' => 25],
            ['name' => 'H510',                      'brand' => 'NZXT',           'category' => 'case',        'price' => 89,   'tray_id' => 26],
            ['name' => 'NR200P',                    'brand' => 'Cooler Master',  'category' => 'case',        'price' => 79,   'tray_id' => 27],
            // Cooling
            ['name' => 'NH-D15',                    'brand' => 'Noctua',         'category' => 'cooling',     'price' => 99,   'tray_id' => 28],
            ['name' => 'Dark Rock Pro 4',           'brand' => 'be quiet!',      'category' => 'cooling',     'price' => 89,   'tray_id' => 29],
            ['name' => 'iCUE H150i 360mm AIO',     'brand' => 'Corsair',        'category' => 'cooling',     'price' => 159,  'tray_id' => 30],
            ['name' => 'AK400',                     'brand' => 'DeepCool',       'category' => 'cooling',     'price' => 39,   'tray_id' => 31],
        ];

        foreach ($components as $data) {
            Component::create([
                'name'        => $data['name'],
                'brand'       => $data['brand'],
                'category_id' => $categories[$data['category']]->id,
                'price'       => $data['price'],
                'tray_id'     => $data['tray_id'],
            ]);
        }
    }
}
