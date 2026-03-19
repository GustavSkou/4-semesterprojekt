<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Log extends Model
{
    protected $fillable = [
        'timestamp',
        'description',
        'level_id',
        'source_id',
        'type_id',
    ];

    public function level()
    {
        return $this->belongsTo(Level::class);
    }

    public function source()
    {
        return $this->belongsTo(Source::class);
    }

    public function type()
    {
        return $this->belongsTo(Type::class);
    }
}
